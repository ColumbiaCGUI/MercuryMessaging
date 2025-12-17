// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Integration tests for MmExtendableResponder with MercuryMessaging framework

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Integration tests verifying MmExtendableResponder works correctly with
    /// the full MercuryMessaging framework (MmRelayNode, hierarchies, filters, etc.)
    /// </summary>
    [TestFixture]
    public class MmExtendableResponderIntegrationTests
    {
        private GameObject rootObject;
        private GameObject parentObject;
        private GameObject childObject;
        private MmRelayNode rootRelay;
        private MmRelayNode parentRelay;
        private MmRelayNode childRelay;
        private TestExtendableResponder rootResponder;
        private TestExtendableResponder parentResponder;
        private TestExtendableResponder childResponder;

        #region Test Setup

        [SetUp]
        public void SetUp()
        {
            // Create three-level hierarchy: Root > Parent > Child
            rootObject = new GameObject("Root");
            rootRelay = rootObject.AddComponent<MmRelayNode>();
            rootResponder = rootObject.AddComponent<TestExtendableResponder>();

            parentObject = new GameObject("Parent");
            parentObject.transform.SetParent(rootObject.transform);
            parentRelay = parentObject.AddComponent<MmRelayNode>();
            parentResponder = parentObject.AddComponent<TestExtendableResponder>();

            childObject = new GameObject("Child");
            childObject.transform.SetParent(parentObject.transform);
            childRelay = childObject.AddComponent<MmRelayNode>();
            childResponder = childObject.AddComponent<TestExtendableResponder>();

            // Register custom handlers on all responders
            rootResponder.RegisterHandler((MmMethod)1000, rootResponder.OnCustomMethod);
            parentResponder.RegisterHandler((MmMethod)1000, parentResponder.OnCustomMethod);
            childResponder.RegisterHandler((MmMethod)1000, childResponder.OnCustomMethod);

            // Refresh responders on each relay to register them with their local routing tables
            rootRelay.MmRefreshResponders();
            parentRelay.MmRefreshResponders();
            childRelay.MmRefreshResponders();

            // Setup hierarchy
            rootRelay.MmAddToRoutingTable(parentRelay, MmLevelFilter.Child);
            parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
            rootRelay.RefreshParents();
            parentRelay.RefreshParents();
            childRelay.RefreshParents();
        }

        [TearDown]
        public void TearDown()
        {
            if (rootObject != null)
                UnityEngine.Object.DestroyImmediate(rootObject);
        }

        private class TestExtendableResponder : MmExtendableResponder
        {
            public int CustomMethodCalls { get; private set; }
            public int StandardMethodCalls { get; private set; }
            public MmMessage LastMessage { get; private set; }

            public void RegisterHandler(MmMethod method, System.Action<MmMessage> handler)
            {
                RegisterCustomHandler(method, handler);
            }

            public void OnCustomMethod(MmMessage message)
            {
                CustomMethodCalls++;
                LastMessage = message;
            }

            public override void Initialize()
            {
                base.Initialize();
                StandardMethodCalls++;
            }

            public void ResetCounters()
            {
                CustomMethodCalls = 0;
                StandardMethodCalls = 0;
                LastMessage = null;
            }
        }

        #endregion

        #region Message Routing Tests

        [Test]
        public void MmInvoke_RoutesCustomMessage_ThroughHierarchy()
        {
            // Arrange
            var message = new MmMessage { MmMethod = (MmMethod)1000 };
            message.MetadataBlock = new MmMetadataBlock(
                MmTagHelper.Everything,
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local);

            // Act - Send from root
            rootRelay.MmInvoke(message);

            // Assert - All responders should receive it
            Assert.AreEqual(1, rootResponder.CustomMethodCalls, "Root should receive message");
            Assert.AreEqual(1, parentResponder.CustomMethodCalls, "Parent should receive message");
            Assert.AreEqual(1, childResponder.CustomMethodCalls, "Child should receive message");
        }

        [Test]
        public void MmInvoke_ChildFilter_OnlyReachesChildren()
        {
            // Arrange
            var message = new MmMessage { MmMethod = (MmMethod)1000 };
            message.MetadataBlock = new MmMetadataBlock(
                MmTagHelper.Everything,
                MmLevelFilter.Child,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local);

            // Act - Send from root with Child filter
            rootRelay.MmInvoke(message);

            // Assert
            Assert.AreEqual(0, rootResponder.CustomMethodCalls, "Root should not receive (Child filter)");
            Assert.AreEqual(1, parentResponder.CustomMethodCalls, "Parent should receive (is child)");
            Assert.AreEqual(1, childResponder.CustomMethodCalls, "Grandchild should receive (is child)");
        }

        [Test]
        public void MmInvoke_ParentFilter_OnlyReachesParents()
        {
            // Arrange
            var message = new MmMessage { MmMethod = (MmMethod)1000 };
            message.MetadataBlock = new MmMetadataBlock(
                MmTagHelper.Everything,
                MmLevelFilter.Parent,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local);

            // Act - Send from child with Parent filter
            childRelay.MmInvoke(message);

            // Assert - Parent filter reaches DIRECT parent only, not all ancestors
            // Use MmLevelFilter.Ancestors to reach all ancestors (grandparents, etc.)
            Assert.AreEqual(0, rootResponder.CustomMethodCalls, "Root should NOT receive (Parent != Ancestors)");
            Assert.AreEqual(1, parentResponder.CustomMethodCalls, "Parent should receive (is direct parent)");
            Assert.AreEqual(0, childResponder.CustomMethodCalls, "Child should not receive (Parent filter)");
        }

        [Test]
        public void MmInvoke_StandardMethods_RouteCorrectly()
        {
            // Arrange
            var message = new MmMessage { MmMethod = MmMethod.Initialize };
            message.MetadataBlock = new MmMetadataBlock(
                MmTagHelper.Everything,
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local);

            // Act
            rootRelay.MmInvoke(message);

            // Assert - All responders should receive Initialize
            Assert.AreEqual(1, rootResponder.StandardMethodCalls);
            Assert.AreEqual(1, parentResponder.StandardMethodCalls);
            Assert.AreEqual(1, childResponder.StandardMethodCalls);
        }

        #endregion

        #region Tag Filtering Tests

        [Test]
        public void MmInvoke_TagFilter_OnlyMatchingRespondersReceive()
        {
            // Arrange - Set different tags on responders
            rootResponder.Tag = MmTag.Tag0;
            parentResponder.Tag = MmTag.Tag1;
            childResponder.Tag = MmTag.Tag2;
            rootResponder.TagCheckEnabled = true;
            parentResponder.TagCheckEnabled = true;
            childResponder.TagCheckEnabled = true;

            // CRITICAL: Refresh routing tables after changing tags to update cached tag values
            rootRelay.MmRefreshResponders();
            parentRelay.MmRefreshResponders();
            childRelay.MmRefreshResponders();

            var message = new MmMessage { MmMethod = (MmMethod)1000 };
            message.MetadataBlock = new MmMetadataBlock(
                MmTag.Tag1, // Only Tag1 should receive
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local);

            // Act
            rootRelay.MmInvoke(message);

            // Assert
            Assert.AreEqual(0, rootResponder.CustomMethodCalls, "Root (Tag0) should not receive");
            Assert.AreEqual(1, parentResponder.CustomMethodCalls, "Parent (Tag1) should receive");
            Assert.AreEqual(0, childResponder.CustomMethodCalls, "Child (Tag2) should not receive");
        }

        [Test]
        public void MmInvoke_EverythingTag_AllRespondersReceive()
        {
            // Arrange - Different tags but message uses Everything
            rootResponder.Tag = MmTag.Tag0;
            parentResponder.Tag = MmTag.Tag1;
            childResponder.Tag = MmTag.Tag2;
            rootResponder.TagCheckEnabled = true;
            parentResponder.TagCheckEnabled = true;
            childResponder.TagCheckEnabled = true;

            var message = new MmMessage { MmMethod = (MmMethod)1000 };
            message.MetadataBlock = new MmMetadataBlock(
                MmTagHelper.Everything,
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local);

            // Act
            rootRelay.MmInvoke(message);

            // Assert - All should receive
            Assert.AreEqual(1, rootResponder.CustomMethodCalls);
            Assert.AreEqual(1, parentResponder.CustomMethodCalls);
            Assert.AreEqual(1, childResponder.CustomMethodCalls);
        }

        #endregion

        #region Active Filter Tests

        [Test]
        public void MmInvoke_ActiveFilter_SkipsInactiveGameObjects()
        {
            // Arrange
            parentObject.SetActive(false); // Deactivate parent
            var message = new MmMessage { MmMethod = (MmMethod)1000 };
            message.MetadataBlock = new MmMetadataBlock(
                MmTagHelper.Everything,
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.Active,
                MmSelectedFilter.All,
                MmNetworkFilter.Local);

            // Act
            rootRelay.MmInvoke(message);

            // Assert
            Assert.AreEqual(1, rootResponder.CustomMethodCalls, "Root (active) should receive");
            Assert.AreEqual(0, parentResponder.CustomMethodCalls, "Parent (inactive) should not receive");
            Assert.AreEqual(0, childResponder.CustomMethodCalls, "Child (inactive parent) should not receive");
        }

        [Test]
        public void MmInvoke_AllFilter_IncludesInactiveGameObjects()
        {
            // Arrange
            parentObject.SetActive(false); // Deactivate parent
            var message = new MmMessage { MmMethod = (MmMethod)1000 };
            message.MetadataBlock = new MmMetadataBlock(
                MmTagHelper.Everything,
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local);

            // Act
            rootRelay.MmInvoke(message);

            // Assert - All should receive even if inactive
            Assert.AreEqual(1, rootResponder.CustomMethodCalls);
            Assert.AreEqual(1, parentResponder.CustomMethodCalls);
            Assert.AreEqual(1, childResponder.CustomMethodCalls);
        }

        #endregion

        #region Lifecycle Tests

        [Test]
        public void Awake_RegistersHandlers_BeforeFirstMessage()
        {
            // Arrange - Create fresh responder
            var freshObject = new GameObject("FreshObject");
            var freshRelay = freshObject.AddComponent<MmRelayNode>();
            var freshResponder = freshObject.AddComponent<TestExtendableResponder>();

            // Handler registration happens in Awake()
            freshResponder.RegisterHandler((MmMethod)2000, freshResponder.OnCustomMethod);

            freshRelay.MmRefreshResponders();
            freshRelay.RefreshParents();

            // Act - Send message immediately
            var message = new MmMessage { MmMethod = (MmMethod)2000 };
            freshRelay.MmInvoke(message);

            // Assert
            Assert.AreEqual(1, freshResponder.CustomMethodCalls);

            // Cleanup
            UnityEngine.Object.DestroyImmediate(freshObject);
        }

        [Test]
        public void OnDestroy_CleansUp_WithoutErrors()
        {
            // Arrange
            var tempObject = new GameObject("TempObject");
            var tempRelay = tempObject.AddComponent<MmRelayNode>();
            var tempResponder = tempObject.AddComponent<TestExtendableResponder>();
            tempResponder.RegisterHandler((MmMethod)3000, msg => { });

            // Act - Destroy object
            UnityEngine.Object.DestroyImmediate(tempObject);

            // Assert - No exception thrown
            Assert.Pass("Object destroyed without errors");
        }

        #endregion

        #region Compatibility Tests

        [Test]
        public void MixedResponders_BothMmBaseAndMmExtendable_WorkTogether()
        {
            // Arrange - Add MmBaseResponder alongside MmExtendableResponder
            var baseResponder = parentObject.AddComponent<MmBaseResponder>();

            // Can't easily test MmBaseResponder without subclassing, so we'll verify both exist
            Assert.IsNotNull(parentResponder, "MmExtendableResponder should exist");
            Assert.IsNotNull(baseResponder, "MmBaseResponder should exist");

            // Act - Send message
            var message = new MmMessage { MmMethod = (MmMethod)1000 };
            parentRelay.MmInvoke(message);

            // Assert - MmExtendableResponder receives custom method
            Assert.AreEqual(1, parentResponder.CustomMethodCalls);
        }

        [Test]
        public void BackwardCompatibility_ExistingCodeStillWorks()
        {
            // This test verifies that existing code using MmBaseResponder is not affected
            // We do this by ensuring standard methods work identically

            // Arrange
            var message = new MmMessageBool { MmMethod = MmMethod.SetActive, value = false };
            // IMPORTANT: Must use MmActiveFilter.All because Unity automatically sets children
            // inactive when parent is set inactive. Default Active filter would prevent
            // message from reaching children after root is set inactive.
            message.MetadataBlock = new MmMetadataBlock(
                MmTagHelper.Everything,
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,  // Include inactive GameObjects
                MmSelectedFilter.All,
                MmNetworkFilter.Local);

            // Act
            rootRelay.MmInvoke(message);

            // Assert - SetActive still works via fast path
            Assert.IsFalse(rootObject.activeSelf);
            Assert.IsFalse(parentObject.activeSelf);
            Assert.IsFalse(childObject.activeSelf);
        }

        #endregion

        #region Error Resilience Tests

        [Test]
        public void HandlerException_DoesNotBreakMessageRouting()
        {
            // Arrange - Register throwing handler
            bool exceptionHandlerCalled = false;
            parentResponder.RegisterHandler((MmMethod)4000, msg =>
            {
                exceptionHandlerCalled = true;
                throw new System.InvalidOperationException("Test exception");
            });

            var message = new MmMessage { MmMethod = (MmMethod)4000 };

            // Act & Assert - Exception should be caught
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Error in custom handler"));
            Assert.DoesNotThrow(() => rootRelay.MmInvoke(message));
            Assert.IsTrue(exceptionHandlerCalled);

            // Other responders should still work
            parentResponder.ResetCounters();
            var successMessage = new MmMessage { MmMethod = (MmMethod)1000 };
            rootRelay.MmInvoke(successMessage);
            Assert.AreEqual(1, parentResponder.CustomMethodCalls, "Other handlers should still work");
        }

        #endregion
    }
}
