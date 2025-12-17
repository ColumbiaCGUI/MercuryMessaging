// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmNetworkPrediction.cs - Client-side prediction and server reconciliation
// Enables responsive networked gameplay by predicting state locally

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Input snapshot for prediction/reconciliation.
    /// </summary>
    [Serializable]
    public struct MmInputSnapshot
    {
        /// <summary>
        /// Sequence number for this input.
        /// </summary>
        public uint SequenceNumber;

        /// <summary>
        /// Timestamp when input was generated.
        /// </summary>
        public double Timestamp;

        /// <summary>
        /// Movement direction input.
        /// </summary>
        public Vector3 MoveDirection;

        /// <summary>
        /// Rotation input (euler angles delta).
        /// </summary>
        public Vector3 RotationDelta;

        /// <summary>
        /// Action flags (jump, fire, etc.).
        /// </summary>
        public uint ActionFlags;

        /// <summary>
        /// Delta time for this input frame.
        /// </summary>
        public float DeltaTime;

        /// <summary>
        /// Additional custom data.
        /// </summary>
        public byte[] CustomData;
    }

    /// <summary>
    /// State snapshot for prediction/reconciliation.
    /// </summary>
    [Serializable]
    public struct MmStateSnapshot
    {
        /// <summary>
        /// Sequence number this state corresponds to.
        /// </summary>
        public uint SequenceNumber;

        /// <summary>
        /// Server timestamp of this state.
        /// </summary>
        public double ServerTime;

        /// <summary>
        /// Position state.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Rotation state.
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// Velocity state.
        /// </summary>
        public Vector3 Velocity;

        /// <summary>
        /// Angular velocity state.
        /// </summary>
        public Vector3 AngularVelocity;

        /// <summary>
        /// Additional custom state data.
        /// </summary>
        public byte[] CustomState;
    }

    /// <summary>
    /// Client-side prediction system.
    /// Maintains predicted state and handles server reconciliation.
    /// </summary>
    public class MmClientPrediction
    {
        /// <summary>
        /// Maximum number of unacknowledged inputs to keep.
        /// </summary>
        public int MaxPendingInputs = 60;

        /// <summary>
        /// Position error threshold for reconciliation (meters).
        /// </summary>
        public float PositionErrorThreshold = 0.01f;

        /// <summary>
        /// Rotation error threshold for reconciliation (degrees).
        /// </summary>
        public float RotationErrorThreshold = 1.0f;

        /// <summary>
        /// Smooth correction factor (0 = instant, 1 = no correction).
        /// </summary>
        public float CorrectionSmoothing = 0.5f;

        // Input history for reconciliation
        private readonly Queue<MmInputSnapshot> _pendingInputs = new Queue<MmInputSnapshot>();
        private uint _lastInputSequence = 0;
        private uint _lastAcknowledgedSequence = 0;

        // Predicted state
        private MmStateSnapshot _predictedState;
        private MmStateSnapshot _lastServerState;

        // Correction state
        private Vector3 _positionError;
        private Quaternion _rotationError;

        // Callback for applying input to state
        private Func<MmStateSnapshot, MmInputSnapshot, MmStateSnapshot> _simulationStep;

        /// <summary>
        /// Current predicted state.
        /// </summary>
        public MmStateSnapshot PredictedState => _predictedState;

        /// <summary>
        /// Last acknowledged sequence number.
        /// </summary>
        public uint LastAcknowledgedSequence => _lastAcknowledgedSequence;

        /// <summary>
        /// Number of pending unacknowledged inputs.
        /// </summary>
        public int PendingInputCount => _pendingInputs.Count;

        /// <summary>
        /// Initialize with simulation step function.
        /// </summary>
        /// <param name="simulationStep">Function that applies input to state and returns new state</param>
        public void Initialize(Func<MmStateSnapshot, MmInputSnapshot, MmStateSnapshot> simulationStep)
        {
            _simulationStep = simulationStep;
        }

        /// <summary>
        /// Set initial state.
        /// </summary>
        public void SetInitialState(MmStateSnapshot state)
        {
            _predictedState = state;
            _lastServerState = state;
            _lastInputSequence = state.SequenceNumber;
            _lastAcknowledgedSequence = state.SequenceNumber;
        }

        /// <summary>
        /// Record local input and update prediction.
        /// Returns the input snapshot with assigned sequence number.
        /// </summary>
        public MmInputSnapshot RecordInput(Vector3 moveDirection, Vector3 rotationDelta, uint actionFlags, float deltaTime, byte[] customData = null)
        {
            _lastInputSequence++;

            var input = new MmInputSnapshot
            {
                SequenceNumber = _lastInputSequence,
                Timestamp = Time.timeAsDouble,
                MoveDirection = moveDirection,
                RotationDelta = rotationDelta,
                ActionFlags = actionFlags,
                DeltaTime = deltaTime,
                CustomData = customData
            };

            // Apply input to predicted state
            if (_simulationStep != null)
            {
                _predictedState = _simulationStep(_predictedState, input);
                _predictedState.SequenceNumber = input.SequenceNumber;
            }

            // Store for reconciliation
            _pendingInputs.Enqueue(input);

            // Trim old inputs
            while (_pendingInputs.Count > MaxPendingInputs)
            {
                _pendingInputs.Dequeue();
            }

            return input;
        }

        /// <summary>
        /// Process authoritative server state and reconcile.
        /// </summary>
        public void OnServerStateReceived(MmStateSnapshot serverState)
        {
            _lastServerState = serverState;
            _lastAcknowledgedSequence = serverState.SequenceNumber;

            // Remove acknowledged inputs
            while (_pendingInputs.Count > 0 && _pendingInputs.Peek().SequenceNumber <= serverState.SequenceNumber)
            {
                _pendingInputs.Dequeue();
            }

            // Replay unacknowledged inputs
            var replayState = serverState;
            foreach (var input in _pendingInputs)
            {
                if (_simulationStep != null)
                {
                    replayState = _simulationStep(replayState, input);
                }
            }

            // Calculate error
            _positionError = _predictedState.Position - replayState.Position;
            _rotationError = Quaternion.Inverse(replayState.Rotation) * _predictedState.Rotation;

            // Check if correction needed
            float posError = _positionError.magnitude;
            float rotError = Quaternion.Angle(Quaternion.identity, _rotationError);

            if (posError > PositionErrorThreshold || rotError > RotationErrorThreshold)
            {
                // Apply smooth correction
                _predictedState.Position = Vector3.Lerp(replayState.Position, _predictedState.Position, CorrectionSmoothing);
                _predictedState.Rotation = Quaternion.Slerp(replayState.Rotation, _predictedState.Rotation, CorrectionSmoothing);
                _predictedState.Velocity = replayState.Velocity;
                _predictedState.AngularVelocity = replayState.AngularVelocity;
            }
            else
            {
                // Small error, just update state directly
                _predictedState = replayState;
            }
        }

        /// <summary>
        /// Get smoothed display state (for rendering).
        /// </summary>
        public MmStateSnapshot GetDisplayState()
        {
            // Could add additional visual smoothing here
            return _predictedState;
        }

        /// <summary>
        /// Reset prediction state.
        /// </summary>
        public void Reset()
        {
            _pendingInputs.Clear();
            _lastInputSequence = 0;
            _lastAcknowledgedSequence = 0;
            _positionError = Vector3.zero;
            _rotationError = Quaternion.identity;
        }
    }

    /// <summary>
    /// Server-side input processing and state broadcast.
    /// </summary>
    public class MmServerReconciliation
    {
        /// <summary>
        /// Tick rate for server updates (Hz).
        /// </summary>
        public int TickRate = 60;

        /// <summary>
        /// How many past states to keep for rewinding.
        /// </summary>
        public int StateHistorySize = 60;

        // Per-client input queues
        private readonly Dictionary<int, Queue<MmInputSnapshot>> _clientInputs = new Dictionary<int, Queue<MmInputSnapshot>>();

        // State history for lag compensation
        private readonly Queue<MmStateSnapshot> _stateHistory = new Queue<MmStateSnapshot>();

        // Current authoritative state
        private MmStateSnapshot _authoritativeState;
        private uint _currentTick = 0;

        // Simulation callback
        private Func<MmStateSnapshot, MmInputSnapshot, MmStateSnapshot> _simulationStep;

        /// <summary>
        /// Current authoritative state.
        /// </summary>
        public MmStateSnapshot AuthoritativeState => _authoritativeState;

        /// <summary>
        /// Current server tick.
        /// </summary>
        public uint CurrentTick => _currentTick;

        /// <summary>
        /// Initialize with simulation step function.
        /// </summary>
        public void Initialize(Func<MmStateSnapshot, MmInputSnapshot, MmStateSnapshot> simulationStep)
        {
            _simulationStep = simulationStep;
        }

        /// <summary>
        /// Set initial state.
        /// </summary>
        public void SetInitialState(MmStateSnapshot state)
        {
            _authoritativeState = state;
            _currentTick = state.SequenceNumber;
        }

        /// <summary>
        /// Receive input from a client.
        /// </summary>
        public void OnClientInput(int clientId, MmInputSnapshot input)
        {
            if (!_clientInputs.TryGetValue(clientId, out var queue))
            {
                queue = new Queue<MmInputSnapshot>();
                _clientInputs[clientId] = queue;
            }

            queue.Enqueue(input);

            // Limit queue size
            while (queue.Count > 30)
            {
                queue.Dequeue();
            }
        }

        /// <summary>
        /// Process one server tick.
        /// Returns the new authoritative state.
        /// </summary>
        public MmStateSnapshot ProcessTick()
        {
            _currentTick++;

            // Process inputs from all clients
            foreach (var kvp in _clientInputs)
            {
                while (kvp.Value.Count > 0)
                {
                    var input = kvp.Value.Dequeue();
                    if (_simulationStep != null)
                    {
                        _authoritativeState = _simulationStep(_authoritativeState, input);
                    }
                }
            }

            // Update state metadata
            _authoritativeState.SequenceNumber = _currentTick;
            _authoritativeState.ServerTime = Time.timeAsDouble;

            // Store in history
            _stateHistory.Enqueue(_authoritativeState);
            while (_stateHistory.Count > StateHistorySize)
            {
                _stateHistory.Dequeue();
            }

            return _authoritativeState;
        }

        /// <summary>
        /// Get historical state for lag compensation.
        /// </summary>
        public bool TryGetHistoricalState(uint tick, out MmStateSnapshot state)
        {
            foreach (var s in _stateHistory)
            {
                if (s.SequenceNumber == tick)
                {
                    state = s;
                    return true;
                }
            }
            state = default;
            return false;
        }

        /// <summary>
        /// Remove a client.
        /// </summary>
        public void RemoveClient(int clientId)
        {
            _clientInputs.Remove(clientId);
        }

        /// <summary>
        /// Reset all state.
        /// </summary>
        public void Reset()
        {
            _clientInputs.Clear();
            _stateHistory.Clear();
            _currentTick = 0;
        }
    }

    /// <summary>
    /// State interpolation for smooth remote entity display.
    /// </summary>
    public class MmStateInterpolator
    {
        /// <summary>
        /// Interpolation delay (seconds).
        /// </summary>
        public float InterpolationDelay = 0.1f;

        /// <summary>
        /// Extrapolation limit (seconds).
        /// </summary>
        public float MaxExtrapolation = 0.2f;

        // State buffer for interpolation
        private readonly List<MmStateSnapshot> _stateBuffer = new List<MmStateSnapshot>();

        /// <summary>
        /// Add a received state to the buffer.
        /// </summary>
        public void AddState(MmStateSnapshot state)
        {
            // Insert in order
            int insertIndex = _stateBuffer.Count;
            for (int i = 0; i < _stateBuffer.Count; i++)
            {
                if (_stateBuffer[i].ServerTime > state.ServerTime)
                {
                    insertIndex = i;
                    break;
                }
            }
            _stateBuffer.Insert(insertIndex, state);

            // Limit buffer size
            while (_stateBuffer.Count > 30)
            {
                _stateBuffer.RemoveAt(0);
            }
        }

        /// <summary>
        /// Get interpolated state for the given render time.
        /// </summary>
        public MmStateSnapshot GetInterpolatedState(double currentTime)
        {
            double renderTime = currentTime - InterpolationDelay;

            if (_stateBuffer.Count == 0)
            {
                return default;
            }

            if (_stateBuffer.Count == 1)
            {
                return _stateBuffer[0];
            }

            // Find bracketing states
            MmStateSnapshot before = _stateBuffer[0];
            MmStateSnapshot after = _stateBuffer[_stateBuffer.Count - 1];

            for (int i = 0; i < _stateBuffer.Count - 1; i++)
            {
                if (_stateBuffer[i].ServerTime <= renderTime && _stateBuffer[i + 1].ServerTime >= renderTime)
                {
                    before = _stateBuffer[i];
                    after = _stateBuffer[i + 1];
                    break;
                }
            }

            // Extrapolation case
            if (renderTime > after.ServerTime)
            {
                float extrapolateTime = (float)(renderTime - after.ServerTime);
                if (extrapolateTime > MaxExtrapolation)
                {
                    extrapolateTime = MaxExtrapolation;
                }
                return Extrapolate(after, extrapolateTime);
            }

            // Interpolation
            float t = (float)((renderTime - before.ServerTime) / (after.ServerTime - before.ServerTime));
            t = Mathf.Clamp01(t);
            return Interpolate(before, after, t);
        }

        private MmStateSnapshot Interpolate(MmStateSnapshot a, MmStateSnapshot b, float t)
        {
            return new MmStateSnapshot
            {
                SequenceNumber = t < 0.5f ? a.SequenceNumber : b.SequenceNumber,
                ServerTime = Mathf.Lerp((float)a.ServerTime, (float)b.ServerTime, t),
                Position = Vector3.Lerp(a.Position, b.Position, t),
                Rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t),
                Velocity = Vector3.Lerp(a.Velocity, b.Velocity, t),
                AngularVelocity = Vector3.Lerp(a.AngularVelocity, b.AngularVelocity, t)
            };
        }

        private MmStateSnapshot Extrapolate(MmStateSnapshot state, float deltaTime)
        {
            return new MmStateSnapshot
            {
                SequenceNumber = state.SequenceNumber,
                ServerTime = state.ServerTime + deltaTime,
                Position = state.Position + state.Velocity * deltaTime,
                Rotation = state.Rotation * Quaternion.Euler(state.AngularVelocity * deltaTime),
                Velocity = state.Velocity,
                AngularVelocity = state.AngularVelocity
            };
        }

        /// <summary>
        /// Clear the state buffer.
        /// </summary>
        public void Clear()
        {
            _stateBuffer.Clear();
        }
    }
}
