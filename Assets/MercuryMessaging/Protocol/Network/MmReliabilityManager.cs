// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmReliabilityManager.cs - Reliability tiers with acknowledgment system
// Manages message delivery guarantees and retransmission

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Packet types for the reliability system.
    /// </summary>
    public enum MmPacketType : byte
    {
        /// <summary>
        /// Regular data packet.
        /// </summary>
        Data = 0,

        /// <summary>
        /// Acknowledgment packet.
        /// </summary>
        Ack = 1,

        /// <summary>
        /// Negative acknowledgment (request retransmit).
        /// </summary>
        Nack = 2,

        /// <summary>
        /// Ping packet for RTT measurement.
        /// </summary>
        Ping = 3,

        /// <summary>
        /// Pong response to ping.
        /// </summary>
        Pong = 4
    }

    /// <summary>
    /// Header for reliable packets.
    /// </summary>
    public struct MmPacketHeader
    {
        public MmPacketType PacketType;
        public uint SequenceNumber;
        public uint AckBitfield;     // Acknowledges up to 32 packets before AckSequence
        public uint AckSequence;     // Most recent received sequence
        public MmReliability Reliability;

        public const int HeaderSize = 14; // 1 + 4 + 4 + 4 + 1

        public void Write(BinaryWriter writer)
        {
            writer.Write((byte)PacketType);
            writer.Write(SequenceNumber);
            writer.Write(AckBitfield);
            writer.Write(AckSequence);
            writer.Write((byte)Reliability);
        }

        public static MmPacketHeader Read(BinaryReader reader)
        {
            return new MmPacketHeader
            {
                PacketType = (MmPacketType)reader.ReadByte(),
                SequenceNumber = reader.ReadUInt32(),
                AckBitfield = reader.ReadUInt32(),
                AckSequence = reader.ReadUInt32(),
                Reliability = (MmReliability)reader.ReadByte()
            };
        }
    }

    /// <summary>
    /// Pending packet awaiting acknowledgment.
    /// </summary>
    public class MmPendingPacket
    {
        public uint SequenceNumber;
        public byte[] Data;
        public double SendTime;
        public int RetryCount;
        public MmReliability Reliability;
        public int TargetClient;
    }

    /// <summary>
    /// Manages reliable message delivery with acknowledgments.
    /// </summary>
    public class MmReliabilityManager
    {
        /// <summary>
        /// Maximum retries before giving up on a packet.
        /// </summary>
        public int MaxRetries = 5;

        /// <summary>
        /// Initial retransmit timeout in seconds.
        /// </summary>
        public float InitialRTO = 0.2f;

        /// <summary>
        /// Maximum retransmit timeout in seconds.
        /// </summary>
        public float MaxRTO = 5.0f;

        /// <summary>
        /// RTT smoothing factor (EWMA).
        /// </summary>
        public float RTTAlpha = 0.125f;

        /// <summary>
        /// Current smoothed RTT estimate.
        /// </summary>
        public float SmoothedRTT { get; private set; } = 0.1f;

        /// <summary>
        /// RTT variance for timeout calculation.
        /// </summary>
        public float RTTVariance { get; private set; } = 0.05f;

        /// <summary>
        /// Current retransmit timeout.
        /// </summary>
        public float CurrentRTO => Mathf.Clamp(SmoothedRTT + 4 * RTTVariance, InitialRTO, MaxRTO);

        // Sequence tracking
        private uint _nextSequence = 1;
        private uint _lastReceivedSequence = 0;
        private uint _receivedBitfield = 0;

        // Pending packets awaiting ACK
        private readonly Dictionary<uint, MmPendingPacket> _pendingPackets = new Dictionary<uint, MmPendingPacket>();
        private readonly object _lock = new object();

        // Events
        public event Action<uint> OnPacketAcked;
        public event Action<uint> OnPacketLost;
        public event Action<byte[], int> OnRetransmit;

        /// <summary>
        /// Number of packets awaiting acknowledgment.
        /// </summary>
        public int PendingCount
        {
            get
            {
                lock (_lock)
                {
                    return _pendingPackets.Count;
                }
            }
        }

        /// <summary>
        /// Wrap data with reliability header and track for ACK.
        /// </summary>
        public byte[] WrapPacket(byte[] data, MmReliability reliability, int targetClient)
        {
            uint seq = GetNextSequence();

            var header = new MmPacketHeader
            {
                PacketType = MmPacketType.Data,
                SequenceNumber = seq,
                AckBitfield = _receivedBitfield,
                AckSequence = _lastReceivedSequence,
                Reliability = reliability
            };

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                header.Write(writer);
                writer.Write(data);
                var wrappedData = ms.ToArray();

                // Track reliable packets for retransmission
                if (reliability == MmReliability.Reliable)
                {
                    lock (_lock)
                    {
                        _pendingPackets[seq] = new MmPendingPacket
                        {
                            SequenceNumber = seq,
                            Data = wrappedData,
                            SendTime = Time.timeAsDouble,
                            RetryCount = 0,
                            Reliability = reliability,
                            TargetClient = targetClient
                        };
                    }
                }

                return wrappedData;
            }
        }

        /// <summary>
        /// Process received packet and extract data.
        /// </summary>
        public byte[] UnwrapPacket(byte[] packet, out MmPacketHeader header)
        {
            using (var ms = new MemoryStream(packet))
            using (var reader = new BinaryReader(ms))
            {
                header = MmPacketHeader.Read(reader);

                // Process ACKs from header
                ProcessAcks(header.AckSequence, header.AckBitfield);

                // Track received sequence
                if (header.PacketType == MmPacketType.Data)
                {
                    TrackReceived(header.SequenceNumber);
                }

                // Extract payload
                int payloadLength = (int)(ms.Length - ms.Position);
                return reader.ReadBytes(payloadLength);
            }
        }

        /// <summary>
        /// Create an ACK-only packet.
        /// </summary>
        public byte[] CreateAckPacket()
        {
            var header = new MmPacketHeader
            {
                PacketType = MmPacketType.Ack,
                SequenceNumber = 0,
                AckBitfield = _receivedBitfield,
                AckSequence = _lastReceivedSequence,
                Reliability = MmReliability.Unreliable
            };

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                header.Write(writer);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Create a ping packet for RTT measurement.
        /// </summary>
        public byte[] CreatePingPacket(double sendTime)
        {
            var header = new MmPacketHeader
            {
                PacketType = MmPacketType.Ping,
                SequenceNumber = 0,
                AckBitfield = 0,
                AckSequence = 0,
                Reliability = MmReliability.Unreliable
            };

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                header.Write(writer);
                writer.Write(sendTime);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Create a pong response packet.
        /// </summary>
        public byte[] CreatePongPacket(double originalSendTime)
        {
            var header = new MmPacketHeader
            {
                PacketType = MmPacketType.Pong,
                SequenceNumber = 0,
                AckBitfield = 0,
                AckSequence = 0,
                Reliability = MmReliability.Unreliable
            };

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                header.Write(writer);
                writer.Write(originalSendTime);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Process a pong packet and update RTT.
        /// </summary>
        public void ProcessPong(byte[] packet)
        {
            using (var ms = new MemoryStream(packet))
            using (var reader = new BinaryReader(ms))
            {
                var header = MmPacketHeader.Read(reader);
                if (header.PacketType != MmPacketType.Pong) return;

                double sendTime = reader.ReadDouble();
                float rtt = (float)(Time.timeAsDouble - sendTime);
                UpdateRTT(rtt);
            }
        }

        /// <summary>
        /// Update RTT estimate using EWMA.
        /// </summary>
        public void UpdateRTT(float measuredRTT)
        {
            if (SmoothedRTT <= 0)
            {
                SmoothedRTT = measuredRTT;
                RTTVariance = measuredRTT / 2;
            }
            else
            {
                float rttDiff = Mathf.Abs(SmoothedRTT - measuredRTT);
                RTTVariance = (1 - RTTAlpha) * RTTVariance + RTTAlpha * rttDiff;
                SmoothedRTT = (1 - RTTAlpha) * SmoothedRTT + RTTAlpha * measuredRTT;
            }
        }

        /// <summary>
        /// Check for packets that need retransmission.
        /// Call this periodically (e.g., every frame).
        /// </summary>
        public void Update(double currentTime)
        {
            List<MmPendingPacket> toRetransmit = null;
            List<uint> toLost = null;

            lock (_lock)
            {
                foreach (var kvp in _pendingPackets)
                {
                    var pending = kvp.Value;
                    float timeout = CurrentRTO * (1 << pending.RetryCount); // Exponential backoff

                    if (currentTime - pending.SendTime > timeout)
                    {
                        if (pending.RetryCount >= MaxRetries)
                        {
                            // Give up on this packet
                            if (toLost == null) toLost = new List<uint>();
                            toLost.Add(kvp.Key);
                        }
                        else
                        {
                            // Queue for retransmission
                            if (toRetransmit == null) toRetransmit = new List<MmPendingPacket>();
                            toRetransmit.Add(pending);
                        }
                    }
                }

                // Remove lost packets
                if (toLost != null)
                {
                    foreach (var seq in toLost)
                    {
                        _pendingPackets.Remove(seq);
                    }
                }
            }

            // Fire events outside lock
            if (toLost != null)
            {
                foreach (var seq in toLost)
                {
                    OnPacketLost?.Invoke(seq);
                }
            }

            if (toRetransmit != null)
            {
                foreach (var pending in toRetransmit)
                {
                    pending.RetryCount++;
                    pending.SendTime = currentTime;
                    OnRetransmit?.Invoke(pending.Data, pending.TargetClient);
                }
            }
        }

        /// <summary>
        /// Get next sequence number.
        /// </summary>
        private uint GetNextSequence()
        {
            return _nextSequence++;
        }

        /// <summary>
        /// Track a received sequence number.
        /// </summary>
        private void TrackReceived(uint seq)
        {
            if (seq > _lastReceivedSequence)
            {
                // Shift bitfield to accommodate new sequence
                uint shift = seq - _lastReceivedSequence;
                if (shift < 32)
                {
                    _receivedBitfield = (_receivedBitfield << (int)shift) | 1;
                }
                else
                {
                    _receivedBitfield = 1;
                }
                _lastReceivedSequence = seq;
            }
            else if (_lastReceivedSequence - seq < 32)
            {
                // Mark older sequence as received
                uint bit = _lastReceivedSequence - seq;
                _receivedBitfield |= (1u << (int)bit);
            }
        }

        /// <summary>
        /// Process acknowledgments from received packet.
        /// </summary>
        private void ProcessAcks(uint ackSeq, uint ackBits)
        {
            List<uint> acked = null;

            lock (_lock)
            {
                // Check main ack sequence
                if (_pendingPackets.ContainsKey(ackSeq))
                {
                    if (acked == null) acked = new List<uint>();
                    acked.Add(ackSeq);
                    _pendingPackets.Remove(ackSeq);
                }

                // Check bitfield for additional acks
                for (int i = 0; i < 32; i++)
                {
                    if ((ackBits & (1u << i)) != 0)
                    {
                        uint seq = ackSeq - (uint)i - 1;
                        if (_pendingPackets.ContainsKey(seq))
                        {
                            if (acked == null) acked = new List<uint>();
                            acked.Add(seq);
                            _pendingPackets.Remove(seq);
                        }
                    }
                }
            }

            // Fire events outside lock
            if (acked != null)
            {
                foreach (var seq in acked)
                {
                    OnPacketAcked?.Invoke(seq);
                }
            }
        }

        /// <summary>
        /// Reset state (e.g., on disconnect).
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _pendingPackets.Clear();
                _nextSequence = 1;
                _lastReceivedSequence = 0;
                _receivedBitfield = 0;
            }
            SmoothedRTT = 0.1f;
            RTTVariance = 0.05f;
        }
    }
}
