// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmStateDeltaTests.cs - Unit tests for delta compression system

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using MercuryMessaging.Network;

namespace MercuryMessaging.Tests.Network
{
    /// <summary>
    /// Unit tests for MmStateDelta and related classes.
    /// </summary>
    [TestFixture]
    public class MmStateDeltaTests
    {
        private MmStateTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _tracker = new MmStateTracker(100);
        }

        [TearDown]
        public void TearDown()
        {
            _tracker = null;
        }

        #region MmStateTracker Tests

        [Test]
        public void StateTracker_Constructor_SetsNetId()
        {
            Assert.AreEqual(100u, _tracker.NetId);
            Assert.AreEqual(0u, _tracker.CurrentSequence);
            Assert.AreEqual(0u, _tracker.AckedSequence);
        }

        [Test]
        public void StateTracker_RegisterField_TracksFieldType()
        {
            _tracker.RegisterField(0, MmDeltaFieldType.Int);
            _tracker.RegisterField(1, MmDeltaFieldType.Float);
            _tracker.RegisterField(2, MmDeltaFieldType.Vector3);

            // No exception means success
            Assert.Pass();
        }

        [Test]
        public void StateTracker_SetFieldValue_UpdatesState()
        {
            _tracker.RegisterField(0, MmDeltaFieldType.Int);
            _tracker.SetFieldValue(0, 42);

            Assert.IsTrue(_tracker.TryGetFieldValue<int>(0, out int value));
            Assert.AreEqual(42, value);
        }

        [Test]
        public void StateTracker_ComputeDelta_ReturnsChangedFields()
        {
            _tracker.RegisterField(0, MmDeltaFieldType.Int);
            _tracker.RegisterField(1, MmDeltaFieldType.Float);

            _tracker.SetFieldValue(0, 100);
            _tracker.SetFieldValue(1, 3.14f);

            var delta = _tracker.ComputeDelta();

            Assert.IsNotNull(delta);
            Assert.AreEqual(2, delta.ChangedFields.Count);
            Assert.AreEqual(1u, delta.DeltaId);
            Assert.AreEqual(100u, delta.NetId);
        }

        [Test]
        public void StateTracker_ComputeDelta_ReturnsNull_WhenNoChanges()
        {
            _tracker.RegisterField(0, MmDeltaFieldType.Int);
            _tracker.SetFieldValue(0, 100);
            _tracker.ComputeDelta();
            _tracker.MarkAsSent();

            // No changes since last send
            var delta = _tracker.ComputeDelta();
            Assert.IsNull(delta);
        }

        [Test]
        public void StateTracker_MarkAsSent_UpdatesBaseline()
        {
            _tracker.RegisterField(0, MmDeltaFieldType.Int);
            _tracker.SetFieldValue(0, 100);
            _tracker.ComputeDelta();
            _tracker.MarkAsSent();

            // Change value
            _tracker.SetFieldValue(0, 200);
            var delta = _tracker.ComputeDelta();

            Assert.IsNotNull(delta);
            Assert.AreEqual(1, delta.ChangedFields.Count);
        }

        [Test]
        public void StateTracker_CreateFullSnapshot_IncludesAllFields()
        {
            _tracker.RegisterField(0, MmDeltaFieldType.Int);
            _tracker.RegisterField(1, MmDeltaFieldType.String);
            _tracker.SetFieldValue(0, 42);
            _tracker.SetFieldValue(1, "test");

            var snapshot = _tracker.CreateFullSnapshot();

            Assert.IsNotNull(snapshot);
            Assert.IsTrue(snapshot.IsFullSnapshot);
            Assert.AreEqual(2, snapshot.ChangedFields.Count);
        }

        [Test]
        public void StateTracker_ApplyDelta_UpdatesState()
        {
            var sourceTracker = new MmStateTracker(100);
            sourceTracker.RegisterField(0, MmDeltaFieldType.Int);
            sourceTracker.SetFieldValue(0, 999);
            var delta = sourceTracker.CreateFullSnapshot();

            _tracker.RegisterField(0, MmDeltaFieldType.Int);
            _tracker.ApplyDelta(delta);

            Assert.IsTrue(_tracker.TryGetFieldValue<int>(0, out int value));
            Assert.AreEqual(999, value);
        }

        #endregion

        #region MmStateDeltaManager Tests

        [Test]
        public void StateDeltaManager_GetOrCreateTracker_CreatesNewTracker()
        {
            var manager = new MmStateDeltaManager();
            var tracker = manager.GetOrCreateTracker(200);

            Assert.IsNotNull(tracker);
            Assert.AreEqual(200u, tracker.NetId);
            Assert.AreEqual(1, manager.TrackerCount);
        }

        [Test]
        public void StateDeltaManager_GetOrCreateTracker_ReturnsSameTracker()
        {
            var manager = new MmStateDeltaManager();
            var tracker1 = manager.GetOrCreateTracker(200);
            var tracker2 = manager.GetOrCreateTracker(200);

            Assert.AreSame(tracker1, tracker2);
        }

        [Test]
        public void StateDeltaManager_GetTracker_ReturnsNull_WhenNotFound()
        {
            var manager = new MmStateDeltaManager();
            var tracker = manager.GetTracker(999);

            Assert.IsNull(tracker);
        }

        [Test]
        public void StateDeltaManager_RemoveTracker_RemovesFromRegistry()
        {
            var manager = new MmStateDeltaManager();
            manager.GetOrCreateTracker(300);
            manager.RemoveTracker(300);

            Assert.IsNull(manager.GetTracker(300));
            Assert.AreEqual(0, manager.TrackerCount);
        }

        [Test]
        public void StateDeltaManager_ComputeAllDeltas_ReturnsChangedTrackers()
        {
            var manager = new MmStateDeltaManager();

            var tracker1 = manager.GetOrCreateTracker(100);
            tracker1.RegisterField(0, MmDeltaFieldType.Int);
            tracker1.SetFieldValue(0, 1);

            var tracker2 = manager.GetOrCreateTracker(200);
            tracker2.RegisterField(0, MmDeltaFieldType.Int);
            tracker2.SetFieldValue(0, 2);

            var deltas = manager.ComputeAllDeltas();

            Assert.AreEqual(2, deltas.Count);
        }

        [Test]
        public void StateDeltaManager_ProcessAck_UpdatesAckedSequence()
        {
            var manager = new MmStateDeltaManager();
            var tracker = manager.GetOrCreateTracker(100);
            tracker.RegisterField(0, MmDeltaFieldType.Int);
            tracker.SetFieldValue(0, 1);
            tracker.ComputeDelta();

            manager.ProcessAck(100, 1);

            Assert.AreEqual(1u, tracker.AckedSequence);
        }

        #endregion

        #region Serialization Tests

        [Test]
        public void DeltaSerializer_Serialize_RoundTrips()
        {
            _tracker.RegisterField(0, MmDeltaFieldType.Int);
            _tracker.RegisterField(1, MmDeltaFieldType.Vector3);
            _tracker.SetFieldValue(0, 42);
            _tracker.SetFieldValue(1, new Vector3(1, 2, 3));

            var delta = _tracker.CreateFullSnapshot();
            byte[] data = MmDeltaSerializer.Serialize(delta);
            var result = MmDeltaSerializer.Deserialize(data);

            Assert.AreEqual(delta.DeltaId, result.DeltaId);
            Assert.AreEqual(delta.NetId, result.NetId);
            Assert.AreEqual(delta.IsFullSnapshot, result.IsFullSnapshot);
            Assert.AreEqual(delta.ChangedFields.Count, result.ChangedFields.Count);
        }

        [Test]
        public void DeltaSerializer_SerializeBatch_RoundTrips()
        {
            var deltas = new List<MmStateDelta>();

            for (int i = 0; i < 5; i++)
            {
                var tracker = new MmStateTracker((uint)(100 + i));
                tracker.RegisterField(0, MmDeltaFieldType.Int);
                tracker.SetFieldValue(0, i * 10);
                deltas.Add(tracker.CreateFullSnapshot());
            }

            byte[] data = MmDeltaSerializer.SerializeBatch(deltas);
            var result = MmDeltaSerializer.DeserializeBatch(data);

            Assert.AreEqual(5, result.Count);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual((uint)(100 + i), result[i].NetId);
            }
        }

        [Test]
        public void DeltaSerializer_IsDeltaBatchPacket_DetectsHeader()
        {
            var deltas = new List<MmStateDelta>
            {
                new MmStateDelta { DeltaId = 1, NetId = 100, ChangedFields = new List<MmDeltaField>() }
            };

            byte[] batchData = MmDeltaSerializer.SerializeBatch(deltas);
            byte[] singleData = MmDeltaSerializer.Serialize(deltas[0]);

            Assert.IsTrue(MmDeltaSerializer.IsDeltaBatchPacket(batchData));
            Assert.IsFalse(MmDeltaSerializer.IsDeltaBatchPacket(singleData));
        }

        #endregion
    }
}
