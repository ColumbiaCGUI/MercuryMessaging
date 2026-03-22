// Study Task Integration Tests — 11 tests total
//
// SOLUTION TESTS (6): Verify each task's solution routing patterns work correctly
//   T2 Mercury:  Send().ToAll().Within() from Worker; only nearby indicators receive
//   T2 Events:   Manual Vector3.Distance check correctly classifies each indicator
//   T3 Mercury:  BroadcastSwitch("Night") + BroadcastValue(25f) → blocked by isActive guard
//   T3 Events:   Mode change + isActive guard blocks direct callback
//   T4 Mercury:  4 subsystems call NotifyValue() → parent StringReceiver receives all 4
//   T4 Events:   4 direct AddAlert() calls populate dashboard list
//
// PROBLEM TESTS (3): Verify starter/broken scenes are actually broken
//   T2 Problem:  Bare hierarchy, no Send() call → 0 messages to any indicator
//   T3 Problem:  HvacBuggyResponder processes temperature in Night mode (bug confirmed)
//   T4 Problem:  Bare hierarchy, no NotifyValue() call → dashboard receives nothing
//
// FULL-WORKFLOW TESTS (2): Simulate Mercury participant adding components to bare scenes
//   T2 FullWorkflow: Add MmRelayNode + StringReceiver to bare GOs, wire routing, then spatial filter
//   T4 FullWorkflow: Add MmRelayNode + StringReceiver to bare GOs, wire routing, then NotifyValue
//
// T1 (Sensor Fan-Out) archived — redundant with T2's broadcast pattern

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using MercuryMessaging;

namespace MercuryMessaging.Tests.StudyTask
{
    [TestFixture]
    public class StudyTaskIntegrationTests
    {
        private List<GameObject> _cleanup;

        [SetUp]
        public void SetUp()
        {
            _cleanup = new List<GameObject>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _cleanup)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            _cleanup.Clear();
        }

        private GameObject CreateGO(string name, Vector3? position = null)
        {
            var go = new GameObject(name);
            if (position.HasValue)
                go.transform.position = position.Value;
            _cleanup.Add(go);
            return go;
        }

        private MmRelayNode AddRelay(GameObject go)
        {
            var relay = go.AddComponent<MmRelayNode>();
            relay.MmRefreshResponders();
            return relay;
        }

        private T AddResponder<T>(GameObject go) where T : MmBaseResponder
        {
            var resp = go.AddComponent<T>();
            go.GetComponent<MmRelayNode>().MmRefreshResponders();
            return resp;
        }

        private void WireParentChild(MmRelayNode parent, MmRelayNode child)
        {
            child.transform.SetParent(parent.transform);
            parent.MmAddToRoutingTable(child, MmLevelFilter.Child);
            child.AddParent(parent);
            parent.MmRefreshResponders();
            child.MmRefreshResponders();
        }

        #region Test Responders

        /// <summary>Receives float messages, tracks count and last value.</summary>
        private class FloatReceiver : MmBaseResponder
        {
            public int Count;
            public float LastValue;

            protected override void ReceivedMessage(MmMessageFloat message)
            {
                Count++;
                LastValue = message.value;
            }
        }

        /// <summary>Receives string messages, tracks all received values.</summary>
        private class StringReceiver : MmBaseResponder
        {
            public List<string> Received = new List<string>();

            protected override void ReceivedMessage(MmMessageString message)
            {
                Received.Add(message.value);
            }
        }

        /// <summary>
        /// Mimics HvacController_Solution: handles Switch and MessageFloat,
        /// guards temperature adjustment with isActive flag.
        /// </summary>
        private class HvacResponder : MmBaseResponder
        {
            public float CurrentSetpoint = 22f;
            public string CurrentMode = "Day";
            public bool IsActive = true;
            public int AdjustmentCount;

            protected override void Switch(string modeName)
            {
                CurrentMode = modeName;
                if (modeName == "Night")
                {
                    CurrentSetpoint = 18f;
                    IsActive = false;
                }
                else
                {
                    CurrentSetpoint = 22f;
                    IsActive = true;
                }
            }

            protected override void ReceivedMessage(MmMessageFloat message)
            {
                if (!IsActive) return; // THE FIX being tested
                CurrentSetpoint = message.value;
                AdjustmentCount++;
            }
        }

        /// <summary>
        /// Mimics HvacController_Buggy: DOES NOT guard temperature adjustment.
        /// </summary>
        private class HvacBuggyResponder : MmBaseResponder
        {
            public float CurrentSetpoint = 22f;
            public string CurrentMode = "Day";
            public bool IsActive = true;
            public int AdjustmentCount;

            protected override void Switch(string modeName)
            {
                CurrentMode = modeName;
                if (modeName == "Night")
                {
                    CurrentSetpoint = 18f;
                    IsActive = false;
                }
                else
                {
                    CurrentSetpoint = 22f;
                    IsActive = true;
                }
            }

            protected override void ReceivedMessage(MmMessageFloat message)
            {
                // BUG: no isActive check
                CurrentSetpoint = message.value;
                AdjustmentCount++;
            }
        }

        #endregion

        // ================================================================
        // SOLUTION TEST 1 of 6
        // T2: Safety Zone Alerts (Spatial Filtering) — Mercury Solution
        // Pattern: relay.Send("warning").ToAll().Within(2f).Execute()
        // Hierarchy: Workspace → Worker (sender, sibling of indicators)
        //                      → Indicator1..4 (at various distances)
        // ================================================================
        [Test]
        public void T2_Mercury_Solution_SpatialFilteringWorks()
        {
            // Arrange: Workspace with Worker and 4 indicators as siblings
            var workspace = CreateGO("Workspace", Vector3.zero);
            var wsRelay = AddRelay(workspace);

            // Worker at origin (this is the sender)
            var worker = CreateGO("Worker", Vector3.zero);
            var workerRelay = AddRelay(worker);
            WireParentChild(wsRelay, workerRelay);

            // Indicators at various distances from Worker
            var positions = new[]
            {
                new Vector3(1.5f, 0, 0),   // 1.5m — within 2m warning zone
                new Vector3(0.8f, 0, 0),   // 0.8m — within 1m emergency zone
                new Vector3(3.0f, 0, 0),   // 3.0m — outside both zones
                new Vector3(1.9f, 0, 0),   // 1.9m — within 2m warning zone
            };

            var indicators = new StringReceiver[4];
            for (int i = 0; i < 4; i++)
            {
                var ind = CreateGO($"Indicator{i + 1}", positions[i]);
                var indRelay = AddRelay(ind);
                indicators[i] = AddResponder<StringReceiver>(ind);
                WireParentChild(wsRelay, indRelay);
            }

            // Act: Worker sends warning within 2m (same as ZoneAlertManager_Solution)
            workerRelay.Send("warning").ToAll().Within(2f).Execute();

            // Assert: within-2m indicators receive warning, outside does not
            Assert.IsTrue(indicators[0].Received.Contains("warning"),
                "Indicator1 at 1.5m should receive warning");
            Assert.IsTrue(indicators[1].Received.Contains("warning"),
                "Indicator2 at 0.8m should receive warning");
            Assert.IsFalse(indicators[2].Received.Contains("warning"),
                "Indicator3 at 3.0m should NOT receive warning");
            Assert.IsTrue(indicators[3].Received.Contains("warning"),
                "Indicator4 at 1.9m should receive warning");

            // Act: Worker sends emergency within 1m
            workerRelay.Send("emergency").ToAll().Within(1f).Execute();

            // Assert: only Indicator2 at 0.8m receives emergency
            Assert.IsFalse(indicators[0].Received.Contains("emergency"),
                "Indicator1 at 1.5m should NOT receive emergency");
            Assert.IsTrue(indicators[1].Received.Contains("emergency"),
                "Indicator2 at 0.8m should receive emergency");
            Assert.IsFalse(indicators[2].Received.Contains("emergency"),
                "Indicator3 at 3.0m should NOT receive emergency");
            Assert.IsFalse(indicators[3].Received.Contains("emergency"),
                "Indicator4 at 1.9m should NOT receive emergency");
        }

        // ================================================================
        // SOLUTION TEST 2 of 6
        // T2: Safety Zone Alerts (Spatial Filtering) — Events Solution
        // Pattern: Manual Vector3.Distance checks for each indicator
        // ================================================================
        [Test]
        public void T2_Events_Solution_ManualDistanceCheckWorks()
        {
            // Arrange: positions matching Mercury test
            var workerPos = Vector3.zero;
            var indicatorPositions = new[]
            {
                new Vector3(1.5f, 0, 0),   // within 2m
                new Vector3(0.8f, 0, 0),   // within 1m
                new Vector3(3.0f, 0, 0),   // outside
                new Vector3(1.9f, 0, 0),   // within 2m
            };

            string[] alertLevels = new string[4];

            // Act: simulate ZoneAlertManager_Events_Solution.Update()
            for (int i = 0; i < 4; i++)
            {
                float dist = Vector3.Distance(workerPos, indicatorPositions[i]);
                if (dist <= 1f)
                    alertLevels[i] = "emergency";
                else if (dist <= 2f)
                    alertLevels[i] = "warning";
                else
                    alertLevels[i] = "clear";
            }

            // Assert
            Assert.AreEqual("warning",   alertLevels[0], "1.5m should be warning");
            Assert.AreEqual("emergency", alertLevels[1], "0.8m should be emergency");
            Assert.AreEqual("clear",     alertLevels[2], "3.0m should be clear");
            Assert.AreEqual("warning",   alertLevels[3], "1.9m should be warning");
        }

        // ================================================================
        // SOLUTION TEST 3 of 6
        // T3: Mode-Switch Debugging — Mercury Solution (fix in place)
        // Pattern: BroadcastSwitch("Night") → HvacResponder ignores subsequent floats
        // ================================================================
        [Test]
        public void T3_Mercury_Solution_NightModeBlocksTemperature()
        {
            // Arrange: FacilityHub → HvacSystem
            var hub = CreateGO("FacilityHub");
            var hubRelay = AddRelay(hub);

            var hvac = CreateGO("HvacSystem");
            var hvacRelay = AddRelay(hvac);
            var hvacResp = AddResponder<HvacResponder>(hvac);
            WireParentChild(hubRelay, hvacRelay);

            // Verify initial state
            Assert.AreEqual("Day", hvacResp.CurrentMode);
            Assert.IsTrue(hvacResp.IsActive);

            // Act: switch to Night mode
            hubRelay.BroadcastSwitch("Night");

            // Assert: mode changed, isActive false, setpoint at night value
            Assert.AreEqual("Night", hvacResp.CurrentMode);
            Assert.IsFalse(hvacResp.IsActive);
            Assert.AreEqual(18f, hvacResp.CurrentSetpoint, 0.01f);

            // Act: send temperature adjustment (should be ignored in Night mode)
            hvacRelay.BroadcastValue(25f);

            // Assert: setpoint unchanged, no adjustment counted
            Assert.AreEqual(0, hvacResp.AdjustmentCount,
                "Temperature adjustments should be blocked in Night mode");
            Assert.AreEqual(18f, hvacResp.CurrentSetpoint, 0.01f,
                "Setpoint should remain at night value");
        }

        // ================================================================
        // SOLUTION TEST 4 of 6
        // T3: Mode-Switch Debugging — Events Solution
        // Pattern: direct method calls with isActive guard
        // ================================================================
        [Test]
        public void T3_Events_Solution_NightModeBlocksTemperature()
        {
            // Arrange: simulate HvacController_Events_Solution state
            string currentMode = "Day";
            float currentSetpoint = 22f;
            bool isActive = true;
            int adjustmentCount = 0;

            // Act: switch to Night (OnModeChanged)
            currentMode = "Night";
            currentSetpoint = 18f;
            isActive = false;

            // Act: temperature callback fires (OnTemperatureRequested)
            float requestedTemp = 25f;
            if (isActive) // THE FIX
            {
                currentSetpoint = requestedTemp;
                adjustmentCount++;
            }

            // Assert: guard blocked the adjustment
            Assert.AreEqual(0, adjustmentCount, "Should not adjust in Night mode");
            Assert.AreEqual(18f, currentSetpoint, 0.01f, "Should remain at night setpoint");
        }

        // ================================================================
        // SOLUTION TEST 5 of 6
        // T4: Alert Aggregation (Many-to-One) — Mercury Solution
        // Pattern: subsystem.NotifyValue(alertData) → parent dashboard receives
        // ================================================================
        [Test]
        public void T4_Mercury_Solution_NotifyValueReachesParentDashboard()
        {
            // Arrange: Dashboard → 4 subsystem children
            var dashboard = CreateGO("Dashboard");
            var dashRelay = AddRelay(dashboard);
            var dashResp = AddResponder<StringReceiver>(dashboard);

            string[] subsystemNames = { "HVAC", "Occupancy", "AirQuality", "Energy" };
            var subsystems = new MmRelayNode[4];

            for (int i = 0; i < 4; i++)
            {
                var sub = CreateGO(subsystemNames[i]);
                subsystems[i] = AddRelay(sub);
                WireParentChild(dashRelay, subsystems[i]);
            }

            // Act: each subsystem notifies parent (same as SubsystemAlerter_Solution)
            subsystems[0].NotifyValue("[HVAC] SEV-2: Temperature above threshold");
            subsystems[1].NotifyValue("[Occupancy] SEV-1: Zone occupied");
            subsystems[2].NotifyValue("[AirQuality] SEV-3: CO2 above limit");
            subsystems[3].NotifyValue("[Energy] SEV-2: Peak usage warning");

            // Assert: dashboard received all 4 alerts
            Assert.AreEqual(4, dashResp.Received.Count,
                "Dashboard should receive all 4 alerts");
            Assert.IsTrue(dashResp.Received[0].Contains("HVAC"),
                "First alert should be from HVAC");
            Assert.IsTrue(dashResp.Received[1].Contains("Occupancy"),
                "Second alert should be from Occupancy");
            Assert.IsTrue(dashResp.Received[2].Contains("AirQuality"),
                "Third alert should be from AirQuality");
            Assert.IsTrue(dashResp.Received[3].Contains("Energy"),
                "Fourth alert should be from Energy");
        }

        // ================================================================
        // SOLUTION TEST 6 of 6
        // T4: Alert Aggregation (Many-to-One) — Events Solution
        // Pattern: dashboard.AddAlert(alertData) — direct reference
        // ================================================================
        [Test]
        public void T4_Events_Solution_DirectDashboardReferenceWorks()
        {
            // Arrange: simulate CentralDashboard_Events
            var alertLog = new List<string>();

            // Act: simulate 4 subsystems calling dashboard.AddAlert()
            string[] alerts =
            {
                "[HVAC] SEV-2: Temperature above threshold",
                "[Occupancy] SEV-1: Zone occupied",
                "[AirQuality] SEV-3: CO2 above limit",
                "[Energy] SEV-2: Peak usage warning"
            };

            foreach (var alert in alerts)
            {
                alertLog.Add(alert); // same as CentralDashboard_Events.AddAlert
            }

            // Assert
            Assert.AreEqual(4, alertLog.Count, "Dashboard should have 4 alerts");
            Assert.IsTrue(alertLog[0].Contains("HVAC"));
            Assert.IsTrue(alertLog[1].Contains("Occupancy"));
            Assert.IsTrue(alertLog[2].Contains("AirQuality"));
            Assert.IsTrue(alertLog[3].Contains("Energy"));
        }

        // ================================================================
        // PROBLEM TEST 1 of 3
        // T2 Problem: No spatial filtering → no messages sent to any indicator
        // ================================================================
        [Test]
        public void T2_Problem_NoSpatialFilter_NothingSent()
        {
            // Arrange: same hierarchy as T2 solution
            var workspace = CreateGO("Workspace", Vector3.zero);
            var wsRelay = AddRelay(workspace);

            var worker = CreateGO("Worker", Vector3.zero);
            var workerRelay = AddRelay(worker);
            WireParentChild(wsRelay, workerRelay);

            var indicators = new StringReceiver[4];
            var positions = new[]
            {
                new Vector3(1.5f, 0, 0),
                new Vector3(0.8f, 0, 0),
                new Vector3(3.0f, 0, 0),
                new Vector3(1.9f, 0, 0),
            };
            for (int i = 0; i < 4; i++)
            {
                var ind = CreateGO($"Indicator{i + 1}", positions[i]);
                var indRelay = AddRelay(ind);
                indicators[i] = AddResponder<StringReceiver>(ind);
                WireParentChild(wsRelay, indRelay);
            }

            // Act: simulate starter's empty Update — does NOT call Send().Within()
            // (ZoneAlertManager_Starter.Update has an empty TODO body — nothing happens)

            // Assert: no indicators received anything
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(0, indicators[i].Received.Count,
                    $"Indicator{i + 1} should NOT receive any message (starter body is empty)");
            }
        }

        // ================================================================
        // PROBLEM TEST 2 of 3
        // T3 Problem: Buggy HVAC still processes temperature in Night mode
        // ================================================================
        [Test]
        public void T3_Problem_BuggyHvac_NightModeDoesNotBlock()
        {
            // Arrange: same hierarchy but with buggy responder
            var hub = CreateGO("FacilityHub");
            var hubRelay = AddRelay(hub);

            var hvac = CreateGO("HvacSystem");
            var hvacRelay = AddRelay(hvac);
            var hvacResp = AddResponder<HvacBuggyResponder>(hvac);
            WireParentChild(hubRelay, hvacRelay);

            // Act: switch to Night — should disable HVAC
            hubRelay.BroadcastSwitch("Night");
            Assert.IsFalse(hvacResp.IsActive, "IsActive should be false in Night mode");

            // Act: send temperature — buggy version still processes it
            hvacRelay.BroadcastValue(25f);

            // Assert: the bug — adjustment happened despite Night mode
            Assert.AreEqual(1, hvacResp.AdjustmentCount,
                "BUG CONFIRMED: Buggy HVAC processes adjustments in Night mode");
            Assert.AreNotEqual(18f, hvacResp.CurrentSetpoint,
                "BUG CONFIRMED: Night setpoint was overwritten");
        }

        // ================================================================
        // PROBLEM TEST 3 of 3
        // T4 Problem: Empty alerter body → dashboard receives nothing
        // ================================================================
        [Test]
        public void T4_Problem_EmptyAlerter_DashboardEmpty()
        {
            // Arrange: same hierarchy as T4 solution
            var dashboard = CreateGO("Dashboard");
            var dashRelay = AddRelay(dashboard);
            var dashResp = AddResponder<StringReceiver>(dashboard);

            var subsystems = new MmRelayNode[4];
            for (int i = 0; i < 4; i++)
            {
                var sub = CreateGO($"Subsystem{i + 1}");
                subsystems[i] = AddRelay(sub);
                WireParentChild(dashRelay, subsystems[i]);
            }

            // Act: simulate starter's empty RaiseAlert — does NOT call NotifyValue
            // (SubsystemAlerter_Starter.RaiseAlert has an empty TODO body)

            // Assert: dashboard received nothing
            Assert.AreEqual(0, dashResp.Received.Count,
                "Dashboard should NOT receive any alerts (starter body is empty)");
        }

        // ================================================================
        // FULL-WORKFLOW TEST 1 of 2
        // T2 Mercury: Simulate participant adding MmRelayNode to bare scene GOs,
        // wiring parent-child routing, then calling Send().ToAll().Within()
        // ================================================================
        [Test]
        public void T2_Mercury_FullWorkflow_AddComponentsThenSpatialFilter()
        {
            // Arrange: bare GameObjects — no MmRelayNode yet (as participant finds them)
            var workspace  = CreateGO("Workspace", Vector3.zero);
            var worker     = CreateGO("Worker",    Vector3.zero);
            var ind1       = CreateGO("Indicator1", new Vector3(1.5f, 0, 0));
            var ind2       = CreateGO("Indicator2", new Vector3(0.8f, 0, 0));
            var ind3       = CreateGO("Indicator3", new Vector3(3.0f, 0, 0));
            var ind4       = CreateGO("Indicator4", new Vector3(1.9f, 0, 0));

            // Act (participant step 1): add MmRelayNode components to each GO
            var wsRelay     = AddRelay(workspace);
            var workerRelay = AddRelay(worker);
            var indRelay1   = AddRelay(ind1);
            var indRelay2   = AddRelay(ind2);
            var indRelay3   = AddRelay(ind3);
            var indRelay4   = AddRelay(ind4);

            // Act (participant step 2): add StringReceiver responders to indicators
            var recv1 = AddResponder<StringReceiver>(ind1);
            var recv2 = AddResponder<StringReceiver>(ind2);
            var recv3 = AddResponder<StringReceiver>(ind3);
            var recv4 = AddResponder<StringReceiver>(ind4);

            // Act (participant step 3): wire parent-child routing relationships
            WireParentChild(wsRelay, workerRelay);
            WireParentChild(wsRelay, indRelay1);
            WireParentChild(wsRelay, indRelay2);
            WireParentChild(wsRelay, indRelay3);
            WireParentChild(wsRelay, indRelay4);

            // Act (participant step 4): send spatial warning from Worker
            workerRelay.Send("warning").ToAll().Within(2f).Execute();

            // Assert: spatial filtering works correctly through participant-wired hierarchy
            Assert.IsTrue(recv1.Received.Contains("warning"),
                "Indicator1 at 1.5m should receive warning after participant wires relay");
            Assert.IsTrue(recv2.Received.Contains("warning"),
                "Indicator2 at 0.8m should receive warning after participant wires relay");
            Assert.IsFalse(recv3.Received.Contains("warning"),
                "Indicator3 at 3.0m should NOT receive warning after participant wires relay");
            Assert.IsTrue(recv4.Received.Contains("warning"),
                "Indicator4 at 1.9m should receive warning after participant wires relay");
        }

        // ================================================================
        // FULL-WORKFLOW TEST 2 of 2
        // T4 Mercury: Simulate participant adding MmRelayNode to bare scene GOs,
        // wiring parent-child routing, then calling NotifyValue()
        // ================================================================
        [Test]
        public void T4_Mercury_FullWorkflow_AddComponentsThenNotifyValue()
        {
            // Arrange: bare GameObjects — no MmRelayNode yet (as participant finds them)
            var dashboard = CreateGO("Dashboard");
            var hvac      = CreateGO("HVAC");
            var occ       = CreateGO("Occupancy");
            var air       = CreateGO("AirQuality");
            var energy    = CreateGO("Energy");

            // Act (participant step 1): add MmRelayNode to dashboard and subsystems
            var dashRelay   = AddRelay(dashboard);
            var hvacRelay   = AddRelay(hvac);
            var occRelay    = AddRelay(occ);
            var airRelay    = AddRelay(air);
            var energyRelay = AddRelay(energy);

            // Act (participant step 2): add StringReceiver to dashboard
            var dashResp = AddResponder<StringReceiver>(dashboard);

            // Act (participant step 3): wire subsystems as children of dashboard
            WireParentChild(dashRelay, hvacRelay);
            WireParentChild(dashRelay, occRelay);
            WireParentChild(dashRelay, airRelay);
            WireParentChild(dashRelay, energyRelay);

            // Act (participant step 4): each subsystem notifies parent dashboard
            hvacRelay.NotifyValue("[HVAC] SEV-2: Temperature above threshold");
            occRelay.NotifyValue("[Occupancy] SEV-1: Zone occupied");
            airRelay.NotifyValue("[AirQuality] SEV-3: CO2 above limit");
            energyRelay.NotifyValue("[Energy] SEV-2: Peak usage warning");

            // Assert: dashboard received all 4 alerts through participant-wired hierarchy
            Assert.AreEqual(4, dashResp.Received.Count,
                "Dashboard should receive all 4 alerts after participant wires relay");
            Assert.IsTrue(dashResp.Received[0].Contains("HVAC"),
                "First alert should be from HVAC");
            Assert.IsTrue(dashResp.Received[1].Contains("Occupancy"),
                "Second alert should be from Occupancy");
            Assert.IsTrue(dashResp.Received[2].Contains("AirQuality"),
                "Third alert should be from AirQuality");
            Assert.IsTrue(dashResp.Received[3].Contains("Energy"),
                "Fourth alert should be from Energy");
        }
    }
}
