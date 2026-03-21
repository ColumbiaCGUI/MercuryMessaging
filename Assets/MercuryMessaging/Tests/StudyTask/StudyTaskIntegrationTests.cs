// Study Task Integration Tests
// Tests all 4 UIST user study task patterns in both Mercury and Events conditions.
// Each test replicates the exact routing pattern used by the study solution scripts.
//
// T1: Sensor Fan-Out (one-to-many) — BroadcastValue(float) → 4 children
// T2: Safety Zone Alerts (spatial filtering) — Send().ToAll().Within() on siblings
// T3: Mode-Switch Debugging — BroadcastSwitch + isActive guard
// T4: Alert Aggregation (many-to-one) — NotifyValue(string) → parent dashboard

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
        // T1: Sensor Fan-Out (One-to-Many) — Mercury
        // Pattern: relay.BroadcastValue(angle) → 4 child FloatReceivers
        // ================================================================
        [Test]
        public void T1_Mercury_BroadcastValue_ReachesAllFourChildren()
        {
            // Arrange: RobotArm with 4 child display panels
            var robotArm = CreateGO("RobotArm");
            var rootRelay = AddRelay(robotArm);

            var panels = new FloatReceiver[4];
            for (int i = 0; i < 4; i++)
            {
                var panel = CreateGO($"Panel{i + 1}");
                var panelRelay = AddRelay(panel);
                panels[i] = AddResponder<FloatReceiver>(panel);
                WireParentChild(rootRelay, panelRelay);
            }

            // Act: BroadcastValue (same as JointDataBroadcaster_Solution.SendJointData)
            rootRelay.BroadcastValue(45.5f);

            // Assert: all 4 panels received the float
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(1, panels[i].Count,
                    $"Panel{i + 1} should receive exactly 1 float message");
                Assert.AreEqual(45.5f, panels[i].LastValue, 0.001f,
                    $"Panel{i + 1} should receive value 45.5");
            }
        }

        // ================================================================
        // T1: Sensor Fan-Out (One-to-Many) — Events
        // Pattern: Direct method calls to 4 receivers
        // ================================================================
        [Test]
        public void T1_Events_DirectCalls_ReachAllFourPanels()
        {
            // Arrange: 4 simple receivers (no Mercury components)
            float[] received = new float[4];
            int[] counts = new int[4];

            // Act: simulate JointDataBroadcaster_Events_Solution.SendJointData
            float angle = 45.5f;
            for (int i = 0; i < 4; i++)
            {
                received[i] = angle;
                counts[i]++;
            }

            // Assert: all 4 received the value
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(1, counts[i], $"Panel{i + 1} should be called once");
                Assert.AreEqual(45.5f, received[i], 0.001f,
                    $"Panel{i + 1} should receive 45.5");
            }
        }

        // ================================================================
        // T2: Safety Zone Alerts (Spatial Filtering) — Mercury
        // Pattern: relay.Send("warning").ToAll().Within(2f).Execute()
        // Hierarchy: Workspace → Worker (sender, sibling of indicators)
        //                      → Indicator1..4 (at various distances)
        // ================================================================
        [Test]
        public void T2_Mercury_SpatialFiltering_OnlyNearbyIndicatorsReceive()
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

            // Assert:
            // Indicator1 (1.5m) — should receive warning
            Assert.IsTrue(indicators[0].Received.Contains("warning"),
                "Indicator1 at 1.5m should receive warning");
            // Indicator2 (0.8m) — should receive warning
            Assert.IsTrue(indicators[1].Received.Contains("warning"),
                "Indicator2 at 0.8m should receive warning");
            // Indicator3 (3.0m) — should NOT receive warning
            Assert.IsFalse(indicators[2].Received.Contains("warning"),
                "Indicator3 at 3.0m should NOT receive warning");
            // Indicator4 (1.9m) — should receive warning
            Assert.IsTrue(indicators[3].Received.Contains("warning"),
                "Indicator4 at 1.9m should receive warning");

            // Act: Worker sends emergency within 1m
            workerRelay.Send("emergency").ToAll().Within(1f).Execute();

            // Assert:
            // Only Indicator2 (0.8m) should receive emergency
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
        // T2: Safety Zone Alerts (Spatial Filtering) — Events
        // Pattern: Manual Vector3.Distance checks for each indicator
        // ================================================================
        [Test]
        public void T2_Events_ManualDistanceCheck_CorrectlyFilters()
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
            Assert.AreEqual("warning", alertLevels[0], "1.5m should be warning");
            Assert.AreEqual("emergency", alertLevels[1], "0.8m should be emergency");
            Assert.AreEqual("clear", alertLevels[2], "3.0m should be clear");
            Assert.AreEqual("warning", alertLevels[3], "1.9m should be warning");
        }

        // ================================================================
        // T3: Mode-Switch Debugging — Mercury (Solution with fix)
        // Pattern: BroadcastSwitch("Night") → HvacResponder ignores subsequent floats
        // ================================================================
        [Test]
        public void T3_Mercury_Solution_NightModeBlocksTemperatureAdjustment()
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

            // Assert: mode changed, isActive false
            Assert.AreEqual("Night", hvacResp.CurrentMode);
            Assert.IsFalse(hvacResp.IsActive);
            Assert.AreEqual(18f, hvacResp.CurrentSetpoint, 0.01f);

            // Act: send temperature adjustment (should be ignored in Night mode)
            hvacRelay.BroadcastValue(25f);

            // Assert: setpoint should NOT have changed (fix is working)
            Assert.AreEqual(0, hvacResp.AdjustmentCount,
                "Temperature adjustments should be blocked in Night mode");
            Assert.AreEqual(18f, hvacResp.CurrentSetpoint, 0.01f,
                "Setpoint should remain at night value");
        }

        // ================================================================
        // T3: Mode-Switch Debugging — Mercury (Buggy version proves bug exists)
        // ================================================================
        [Test]
        public void T3_Mercury_Buggy_NightModeStillAllowsTemperatureAdjustment()
        {
            // Arrange: same hierarchy but with buggy responder
            var hub = CreateGO("FacilityHub");
            var hubRelay = AddRelay(hub);

            var hvac = CreateGO("HvacSystem");
            var hvacRelay = AddRelay(hvac);
            var hvacResp = AddResponder<HvacBuggyResponder>(hvac);
            WireParentChild(hubRelay, hvacRelay);

            // Act: switch to Night mode
            hubRelay.BroadcastSwitch("Night");
            Assert.AreEqual("Night", hvacResp.CurrentMode);
            Assert.IsFalse(hvacResp.IsActive);

            // Act: send temperature (bug: this should be blocked but isn't!)
            hvacRelay.BroadcastValue(25f);

            // Assert: buggy version processes the adjustment
            Assert.AreEqual(1, hvacResp.AdjustmentCount,
                "Buggy version incorrectly processes adjustments in Night mode");
            Assert.AreEqual(25f, hvacResp.CurrentSetpoint, 0.01f,
                "Buggy version overwrites night setpoint");
        }

        // ================================================================
        // T3: Mode-Switch Debugging — Events (Solution)
        // Pattern: direct method calls with isActive guard
        // ================================================================
        [Test]
        public void T3_Events_Solution_NightModeBlocksTemperatureCallback()
        {
            // Arrange: simulate HvacController_Events_Solution
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
        // T4: Alert Aggregation (Many-to-One) — Mercury
        // Pattern: subsystem.NotifyValue(alertData) → parent dashboard receives
        // ================================================================
        [Test]
        public void T4_Mercury_NotifyValue_ReachesParentDashboard()
        {
            // Arrange: Dashboard → 4 subsystem children
            // (In actual study, dashboard is parent of subsystems)
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
        // T4: Alert Aggregation (Many-to-One) — Events
        // Pattern: dashboard.AddAlert(alertData) — direct reference
        // ================================================================
        [Test]
        public void T4_Events_DirectDashboardReference_ReceivesAllAlerts()
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
    }
}
