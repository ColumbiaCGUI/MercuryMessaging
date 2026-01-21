// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmNetworkPredictionTests.cs - Unit tests for prediction and reconciliation

using NUnit.Framework;
using UnityEngine;
using MercuryMessaging.Network;

namespace MercuryMessaging.Tests.Network
{
    /// <summary>
    /// Unit tests for MmNetworkPrediction and related classes.
    /// </summary>
    [TestFixture]
    public class MmNetworkPredictionTests
    {
        #region MmClientPrediction Tests

        [Test]
        public void ClientPrediction_SetInitialState_SetsState()
        {
            var prediction = new MmClientPrediction();

            var initialState = new MmStateSnapshot
            {
                SequenceNumber = 0,
                Position = new Vector3(1, 2, 3),
                Rotation = Quaternion.identity
            };

            prediction.SetInitialState(initialState);

            Assert.AreEqual(new Vector3(1, 2, 3), prediction.PredictedState.Position);
            Assert.AreEqual(0u, prediction.LastAcknowledgedSequence);
        }

        [Test]
        public void ClientPrediction_RecordInput_UpdatesPrediction()
        {
            var prediction = new MmClientPrediction();
            prediction.Initialize(SimulateMovement);
            prediction.SetInitialState(new MmStateSnapshot
            {
                Position = Vector3.zero,
                Rotation = Quaternion.identity
            });

            var input = prediction.RecordInput(
                moveDirection: Vector3.forward,
                rotationDelta: Vector3.zero,
                actionFlags: 0,
                deltaTime: 0.1f
            );

            Assert.AreEqual(1u, input.SequenceNumber);
            Assert.AreEqual(1, prediction.PendingInputCount);
            // Position should have moved forward
            Assert.Greater(prediction.PredictedState.Position.z, 0);
        }

        [Test]
        public void ClientPrediction_OnServerStateReceived_RemovesAckedInputs()
        {
            var prediction = new MmClientPrediction();
            prediction.Initialize(SimulateMovement);
            prediction.SetInitialState(new MmStateSnapshot
            {
                SequenceNumber = 0,
                Position = Vector3.zero,
                Rotation = Quaternion.identity
            });

            // Record several inputs
            prediction.RecordInput(Vector3.forward, Vector3.zero, 0, 0.1f);
            prediction.RecordInput(Vector3.forward, Vector3.zero, 0, 0.1f);
            prediction.RecordInput(Vector3.forward, Vector3.zero, 0, 0.1f);

            Assert.AreEqual(3, prediction.PendingInputCount);

            // Server acknowledges first two
            var serverState = new MmStateSnapshot
            {
                SequenceNumber = 2,
                Position = new Vector3(0, 0, 0.2f),
                Rotation = Quaternion.identity
            };
            prediction.OnServerStateReceived(serverState);

            Assert.AreEqual(1, prediction.PendingInputCount);
            Assert.AreEqual(2u, prediction.LastAcknowledgedSequence);
        }

        [Test]
        public void ClientPrediction_Reset_ClearsState()
        {
            var prediction = new MmClientPrediction();
            prediction.Initialize(SimulateMovement);
            prediction.SetInitialState(new MmStateSnapshot
            {
                Position = Vector3.zero,
                Rotation = Quaternion.identity
            });

            prediction.RecordInput(Vector3.forward, Vector3.zero, 0, 0.1f);
            prediction.Reset();

            Assert.AreEqual(0, prediction.PendingInputCount);
            Assert.AreEqual(0u, prediction.LastAcknowledgedSequence);
        }

        private MmStateSnapshot SimulateMovement(MmStateSnapshot state, MmInputSnapshot input)
        {
            float speed = 1.0f;
            return new MmStateSnapshot
            {
                SequenceNumber = state.SequenceNumber,
                ServerTime = state.ServerTime,
                Position = state.Position + input.MoveDirection * speed * input.DeltaTime,
                Rotation = state.Rotation * Quaternion.Euler(input.RotationDelta * input.DeltaTime),
                Velocity = input.MoveDirection * speed,
                AngularVelocity = input.RotationDelta
            };
        }

        #endregion

        #region MmServerReconciliation Tests

        [Test]
        public void ServerReconciliation_ProcessTick_IncrementsTick()
        {
            var server = new MmServerReconciliation();
            server.Initialize(SimulateMovement);
            server.SetInitialState(new MmStateSnapshot
            {
                SequenceNumber = 0,
                Position = Vector3.zero,
                Rotation = Quaternion.identity
            });

            server.ProcessTick();
            server.ProcessTick();
            server.ProcessTick();

            Assert.AreEqual(3u, server.CurrentTick);
        }

        [Test]
        public void ServerReconciliation_OnClientInput_ProcessesInput()
        {
            var server = new MmServerReconciliation();
            server.Initialize(SimulateMovement);
            server.SetInitialState(new MmStateSnapshot
            {
                SequenceNumber = 0,
                Position = Vector3.zero,
                Rotation = Quaternion.identity
            });

            server.OnClientInput(1, new MmInputSnapshot
            {
                SequenceNumber = 1,
                MoveDirection = Vector3.forward,
                DeltaTime = 0.1f
            });

            var state = server.ProcessTick();

            Assert.Greater(state.Position.z, 0);
        }

        [Test]
        public void ServerReconciliation_TryGetHistoricalState_ReturnsStoredState()
        {
            var server = new MmServerReconciliation();
            server.Initialize(SimulateMovement);
            server.SetInitialState(new MmStateSnapshot
            {
                SequenceNumber = 0,
                Position = Vector3.zero,
                Rotation = Quaternion.identity
            });

            server.ProcessTick();
            server.ProcessTick();
            var state = server.ProcessTick();

            Assert.IsTrue(server.TryGetHistoricalState(state.SequenceNumber, out var historicalState));
            Assert.AreEqual(state.SequenceNumber, historicalState.SequenceNumber);
        }

        [Test]
        public void ServerReconciliation_RemoveClient_ClearsInputQueue()
        {
            var server = new MmServerReconciliation();
            server.Initialize(SimulateMovement);

            server.OnClientInput(1, new MmInputSnapshot { SequenceNumber = 1 });
            server.RemoveClient(1);

            // No exception means success
            Assert.Pass();
        }

        #endregion

        #region MmStateInterpolator Tests

        [Test]
        public void StateInterpolator_AddState_StoresState()
        {
            var interpolator = new MmStateInterpolator();

            interpolator.AddState(new MmStateSnapshot
            {
                ServerTime = 1.0,
                Position = Vector3.zero
            });

            interpolator.AddState(new MmStateSnapshot
            {
                ServerTime = 2.0,
                Position = Vector3.one
            });

            // No exception means success
            Assert.Pass();
        }

        [Test]
        public void StateInterpolator_GetInterpolatedState_InterpolatesBetweenStates()
        {
            var interpolator = new MmStateInterpolator();
            interpolator.InterpolationDelay = 0;

            interpolator.AddState(new MmStateSnapshot
            {
                ServerTime = 0.0,
                Position = Vector3.zero,
                Rotation = Quaternion.identity
            });

            interpolator.AddState(new MmStateSnapshot
            {
                ServerTime = 1.0,
                Position = new Vector3(10, 0, 0),
                Rotation = Quaternion.identity
            });

            var result = interpolator.GetInterpolatedState(0.5);

            Assert.AreEqual(5f, result.Position.x, 0.1f);
        }

        [Test]
        public void StateInterpolator_GetInterpolatedState_ExtrapolatesWhenNeeded()
        {
            var interpolator = new MmStateInterpolator();
            interpolator.InterpolationDelay = 0;
            interpolator.MaxExtrapolation = 0.5f;

            interpolator.AddState(new MmStateSnapshot
            {
                ServerTime = 0.0,
                Position = Vector3.zero,
                Velocity = new Vector3(10, 0, 0),
                Rotation = Quaternion.identity
            });

            interpolator.AddState(new MmStateSnapshot
            {
                ServerTime = 1.0,
                Position = new Vector3(10, 0, 0),
                Velocity = new Vector3(10, 0, 0),
                Rotation = Quaternion.identity
            });

            // Request time beyond last state
            var result = interpolator.GetInterpolatedState(1.2);

            // Should extrapolate
            Assert.Greater(result.Position.x, 10f);
        }

        [Test]
        public void StateInterpolator_Clear_RemovesAllStates()
        {
            var interpolator = new MmStateInterpolator();

            interpolator.AddState(new MmStateSnapshot { ServerTime = 1.0 });
            interpolator.AddState(new MmStateSnapshot { ServerTime = 2.0 });
            interpolator.Clear();

            // Getting state should return default
            var result = interpolator.GetInterpolatedState(1.5);
            Assert.AreEqual(0.0, result.ServerTime);
        }

        #endregion

        #region MmReliabilityManager Tests

        [Test]
        public void ReliabilityManager_WrapPacket_AddsHeader()
        {
            var manager = new MmReliabilityManager();
            byte[] data = new byte[] { 1, 2, 3, 4 };

            byte[] wrapped = manager.WrapPacket(data, MmReliability.Reliable, 0);

            Assert.Greater(wrapped.Length, data.Length);
            Assert.AreEqual(MmPacketHeader.HeaderSize + data.Length, wrapped.Length);
        }

        [Test]
        public void ReliabilityManager_UnwrapPacket_ExtractsData()
        {
            var manager = new MmReliabilityManager();
            byte[] original = new byte[] { 1, 2, 3, 4 };

            byte[] wrapped = manager.WrapPacket(original, MmReliability.Unreliable, 0);
            byte[] unwrapped = manager.UnwrapPacket(wrapped, out var header);

            Assert.AreEqual(original.Length, unwrapped.Length);
            Assert.AreEqual(MmPacketType.Data, header.PacketType);
        }

        [Test]
        public void ReliabilityManager_WrapPacket_TracksReliablePackets()
        {
            var manager = new MmReliabilityManager();
            byte[] data = new byte[] { 1, 2, 3, 4 };

            manager.WrapPacket(data, MmReliability.Reliable, 0);
            manager.WrapPacket(data, MmReliability.Reliable, 0);

            Assert.AreEqual(2, manager.PendingCount);
        }

        [Test]
        public void ReliabilityManager_WrapPacket_DoesNotTrackUnreliable()
        {
            var manager = new MmReliabilityManager();
            byte[] data = new byte[] { 1, 2, 3, 4 };

            manager.WrapPacket(data, MmReliability.Unreliable, 0);

            Assert.AreEqual(0, manager.PendingCount);
        }

        [Test]
        public void ReliabilityManager_CreateAckPacket_ReturnsValidPacket()
        {
            var manager = new MmReliabilityManager();
            byte[] ackPacket = manager.CreateAckPacket();

            Assert.IsNotNull(ackPacket);
            Assert.AreEqual(MmPacketHeader.HeaderSize, ackPacket.Length);
        }

        [Test]
        public void ReliabilityManager_UpdateRTT_SmoothesMeasurement()
        {
            var manager = new MmReliabilityManager();
            float initialRTT = manager.SmoothedRTT;

            manager.UpdateRTT(0.2f);

            Assert.AreNotEqual(initialRTT, manager.SmoothedRTT);
        }

        [Test]
        public void ReliabilityManager_Reset_ClearsState()
        {
            var manager = new MmReliabilityManager();
            manager.WrapPacket(new byte[] { 1 }, MmReliability.Reliable, 0);
            manager.Reset();

            Assert.AreEqual(0, manager.PendingCount);
        }

        #endregion
    }
}
