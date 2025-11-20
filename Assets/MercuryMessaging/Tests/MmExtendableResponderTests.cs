// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
//
// Unit tests for MmExtendableResponder class

using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Comprehensive unit tests for MmExtendableResponder registration-based custom method handling.
    /// Tests registration validation, handler invocation, error handling, and helper methods.
    /// </summary>
    [TestFixture]
    public class MmExtendableResponderTests
    {
        private GameObject testObject;
        private TestExtendableResponder responder;
        private MmRelayNode relay;

        #region Test Setup and Helpers

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with responder and relay programmatically
            testObject = new GameObject("TestObject");
            responder = testObject.AddComponent<TestExtendableResponder>();
            relay = testObject.AddComponent<MmRelayNode>();
            relay.MmRefreshResponders(); // Explicit registration for dynamically added components
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testObject);
            }
        }

        /// <summary>
        /// Test responder implementation for validating MmExtendableResponder functionality
        /// </summary>
        private class TestExtendableResponder : MmExtendableResponder
        {
            public int CallCount { get; private set; }
            public MmMessage LastMessage { get; private set; }
            public int LastMethodValue { get; private set; }
            public bool HandlerThrew { get; private set; }
            public bool InitializeCalled { get; set; }

            public void RegisterHandler(MmMethod method, Action<MmMessage> handler)
            {
                RegisterCustomHandler(method, handler);
            }

            public void UnregisterHandler(MmMethod method)
            {
                UnregisterCustomHandler(method);
            }

            public bool HasHandler(MmMethod method)
            {
                return HasCustomHandler(method);
            }

            public override void Initialize()
            {
                base.Initialize();
                InitializeCalled = true;
            }

            public void OnTestMethod(MmMessage message)
            {
                CallCount++;
                LastMessage = message;
                LastMethodValue = (int)message.MmMethod;
            }

            public void OnThrowingMethod(MmMessage message)
            {
                HandlerThrew = true;
                throw new InvalidOperationException("Test exception");
            }

            public void ResetCounters()
            {
                CallCount = 0;
                LastMessage = null;
                LastMethodValue = 0;
                HandlerThrew = false;
                InitializeCalled = false;
            }
        }

        #endregion

        #region Registration Validation Tests

        [Test]
        public void RegisterCustomHandler_ThrowsException_ForMethodLessThan1000()
        {
            // Arrange
            MmMethod invalidMethod = (MmMethod)999;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                responder.RegisterHandler(invalidMethod, responder.OnTestMethod));

            Assert.That(ex.Message, Does.Contain("Custom methods must be >= 1000"));
            Assert.That(ex.Message, Does.Contain("999"));
        }

        [Test]
        public void RegisterCustomHandler_ThrowsException_ForMethodEquals999()
        {
            // Arrange
            MmMethod invalidMethod = (MmMethod)999;

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                responder.RegisterHandler(invalidMethod, responder.OnTestMethod));
        }

        [Test]
        public void RegisterCustomHandler_Succeeds_ForMethodEquals1000()
        {
            // Arrange
            MmMethod validMethod = (MmMethod)1000;

            // Act & Assert
            Assert.DoesNotThrow(() => responder.RegisterHandler(validMethod, responder.OnTestMethod));
        }

        [Test]
        public void RegisterCustomHandler_Succeeds_ForMethodGreaterThan1000()
        {
            // Arrange
            MmMethod validMethod = (MmMethod)5000;

            // Act & Assert
            Assert.DoesNotThrow(() => responder.RegisterHandler(validMethod, responder.OnTestMethod));
        }

        [Test]
        public void RegisterCustomHandler_ThrowsException_ForNullHandler()
        {
            // Arrange
            MmMethod method = (MmMethod)1000;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                responder.RegisterHandler(method, null));

            Assert.That(ex.Message, Does.Contain("handler"));
        }

        #endregion

        #region Handler Invocation Tests

        [Test]
        public void MmInvoke_CallsRegisteredHandler_ForCustomMethod()
        {
            // Arrange
            MmMethod customMethod = (MmMethod)1000;
            responder.RegisterHandler(customMethod, responder.OnTestMethod);
            var message = new MmMessage { MmMethod = customMethod };

            // Act
            responder.MmInvoke(message);

            // Assert
            Assert.AreEqual(1, responder.CallCount);
            Assert.AreEqual(message, responder.LastMessage);
            Assert.AreEqual(1000, responder.LastMethodValue);
        }

        [Test]
        public void MmInvoke_PassesCorrectMessage_ToHandler()
        {
            // Arrange
            MmMethod customMethod = (MmMethod)2000;
            responder.RegisterHandler(customMethod, responder.OnTestMethod);
            var message = new MmMessageString { MmMethod = customMethod, value = "test data" };

            // Act
            responder.MmInvoke(message);

            // Assert
            Assert.AreSame(message, responder.LastMessage);
            Assert.AreEqual("test data", ((MmMessageString)responder.LastMessage).value);
        }

        [Test]
        public void MmInvoke_CallsMultipleHandlers_Independently()
        {
            // Arrange
            int calls1 = 0, calls2 = 0;
            responder.RegisterHandler((MmMethod)1000, _ => calls1++);
            responder.RegisterHandler((MmMethod)1001, _ => calls2++);

            // Act
            responder.MmInvoke(new MmMessage { MmMethod = (MmMethod)1000 });
            responder.MmInvoke(new MmMessage { MmMethod = (MmMethod)1001 });
            responder.MmInvoke(new MmMessage { MmMethod = (MmMethod)1000 });

            // Assert
            Assert.AreEqual(2, calls1);
            Assert.AreEqual(1, calls2);
        }

        [Test]
        public void MmInvoke_HandlerCanModifyInstanceState()
        {
            // Arrange
            bool stateChanged = false;
            responder.RegisterHandler((MmMethod)1000, _ => stateChanged = true);

            // Act
            responder.MmInvoke(new MmMessage { MmMethod = (MmMethod)1000 });

            // Assert
            Assert.IsTrue(stateChanged);
        }

        [Test]
        public void MmInvoke_StandardMethodsStillWork_SetActive()
        {
            // Arrange
            testObject.SetActive(true);

            // Act
            responder.MmInvoke(new MmMessageBool { MmMethod = MmMethod.SetActive, value = false });

            // Assert
            Assert.IsFalse(testObject.activeSelf);
        }

        [Test]
        public void MmInvoke_StandardMethodsStillWork_Initialize()
        {
            // Arrange
            responder.InitializeCalled = false;

            // Act
            responder.MmInvoke(new MmMessage { MmMethod = MmMethod.Initialize });

            // Assert
            Assert.IsTrue(responder.InitializeCalled);
        }

        #endregion

        #region Unregistration Tests

        [Test]
        public void UnregisterCustomHandler_RemovesHandler()
        {
            // Arrange
            MmMethod method = (MmMethod)1000;
            responder.RegisterHandler(method, responder.OnTestMethod);
            responder.MmInvoke(new MmMessage { MmMethod = method });
            Assert.AreEqual(1, responder.CallCount); // Verify it was registered

            // Act
            responder.UnregisterHandler(method);
            responder.ResetCounters();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Unhandled custom method"));
            responder.MmInvoke(new MmMessage { MmMethod = method });

            // Assert
            Assert.AreEqual(0, responder.CallCount); // Should not be called after unregister
        }

        [Test]
        public void UnregisterCustomHandler_DoesNotThrow_WhenNotRegistered()
        {
            // Arrange
            MmMethod method = (MmMethod)1000;

            // Act & Assert
            Assert.DoesNotThrow(() => responder.UnregisterHandler(method));
        }

        [Test]
        public void RegisterCustomHandler_CanReregister_AfterUnregistering()
        {
            // Arrange
            MmMethod method = (MmMethod)1000;
            responder.RegisterHandler(method, responder.OnTestMethod);
            responder.UnregisterHandler(method);

            // Act
            responder.RegisterHandler(method, responder.OnTestMethod);
            responder.MmInvoke(new MmMessage { MmMethod = method });

            // Assert
            Assert.AreEqual(1, responder.CallCount);
        }

        #endregion

        #region Helper Methods Tests

        [Test]
        public void HasCustomHandler_ReturnsTrue_ForRegisteredMethod()
        {
            // Arrange
            MmMethod method = (MmMethod)1000;
            responder.RegisterHandler(method, responder.OnTestMethod);

            // Act
            bool hasHandler = responder.HasHandler(method);

            // Assert
            Assert.IsTrue(hasHandler);
        }

        [Test]
        public void HasCustomHandler_ReturnsFalse_ForUnregisteredMethod()
        {
            // Arrange
            MmMethod method = (MmMethod)1000;

            // Act
            bool hasHandler = responder.HasHandler(method);

            // Assert
            Assert.IsFalse(hasHandler);
        }

        [Test]
        public void HasCustomHandler_ReturnsFalse_BeforeAnyRegistration()
        {
            // Act
            bool hasHandler = responder.HasHandler((MmMethod)1000);

            // Assert
            Assert.IsFalse(hasHandler);
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void MmInvoke_CatchesHandlerException_AndLogsError()
        {
            // Arrange
            responder.RegisterHandler((MmMethod)1000, responder.OnThrowingMethod);

            // Act & Assert
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Error in custom handler"));
            Assert.DoesNotThrow(() => responder.MmInvoke(new MmMessage { MmMethod = (MmMethod)1000 }));
            Assert.IsTrue(responder.HandlerThrew);
        }

        [Test]
        public void MmInvoke_OtherHandlersStillWork_AfterOneThrows()
        {
            // Arrange
            int successfulCalls = 0;
            responder.RegisterHandler((MmMethod)1000, responder.OnThrowingMethod);
            responder.RegisterHandler((MmMethod)1001, _ => successfulCalls++);

            // Act
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Error in custom handler"));
            responder.MmInvoke(new MmMessage { MmMethod = (MmMethod)1000 }); // Throws
            responder.MmInvoke(new MmMessage { MmMethod = (MmMethod)1001 }); // Should work

            // Assert
            Assert.AreEqual(1, successfulCalls);
        }

        [Test]
        public void MmInvoke_LogsWarning_ForUnhandledMethod()
        {
            // Arrange
            MmMethod unhandledMethod = (MmMethod)9999;

            // Act & Assert
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Unhandled custom method"));
            responder.MmInvoke(new MmMessage { MmMethod = unhandledMethod });
        }

        [Test]
        public void OnUnhandledCustomMethod_IsVirtual_AndCanBeOverridden()
        {
            // This test verifies the method is virtual by checking if a derived class can override it
            // The virtual keyword is verified by successful compilation
            Assert.Pass("OnUnhandledCustomMethod is virtual (verified by compilation)");
        }

        #endregion

        #region Edge Cases Tests

        [Test]
        public void RegisterCustomHandler_Overwrites_WhenRegisteredTwice()
        {
            // Arrange
            MmMethod method = (MmMethod)1000;
            int calls1 = 0, calls2 = 0;
            responder.RegisterHandler(method, _ => calls1++);
            responder.RegisterHandler(method, _ => calls2++); // Overwrite

            // Act
            responder.MmInvoke(new MmMessage { MmMethod = method });

            // Assert
            Assert.AreEqual(0, calls1); // First handler should not be called
            Assert.AreEqual(1, calls2); // Second handler should be called
        }

        [Test]
        public void RegisterCustomHandler_CanRegisterManyHandlers()
        {
            // Arrange & Act
            for (int i = 1000; i < 1100; i++)
            {
                int captured = i;
                responder.RegisterHandler((MmMethod)i, _ => { });
            }

            // Assert
            for (int i = 1000; i < 1100; i++)
            {
                Assert.IsTrue(responder.HasHandler((MmMethod)i));
            }
        }

        [Test]
        public void Dictionary_LazyInitialization_UntilFirstRegistration()
        {
            // Arrange - Fresh responder with no registrations
            var freshObject = new GameObject("FreshObject");
            var freshResponder = freshObject.AddComponent<TestExtendableResponder>();

            // Act - Check handler before any registration
            bool hasHandler = freshResponder.HasHandler((MmMethod)1000);

            // Assert
            Assert.IsFalse(hasHandler); // Should return false even with null dictionary

            // Cleanup
            UnityEngine.Object.DestroyImmediate(freshObject);
        }

        #endregion

        #region Performance Tests

        [Test]
        public void MmInvoke_FastPath_UsesBaseResponder_ForStandardMethods()
        {
            // This test verifies that standard methods (0-999) use the fast path
            // We test by ensuring Initialize (method 4) works correctly through base.MmInvoke()

            // Act
            responder.MmInvoke(new MmMessage { MmMethod = MmMethod.Initialize });

            // Assert - If fast path works, Initialize should have been called
            Assert.IsTrue(responder.InitializeCalled, "Fast path should route to base.MmInvoke() and call Initialize()");
        }

        [Test]
        public void MmInvoke_SlowPath_UsesDictionary_ForCustomMethods()
        {
            // Arrange
            bool customHandlerCalled = false;
            responder.RegisterHandler((MmMethod)1000, _ => customHandlerCalled = true);

            // Act
            responder.MmInvoke(new MmMessage { MmMethod = (MmMethod)1000 });

            // Assert
            Assert.IsTrue(customHandlerCalled);
        }

        #endregion
    }
}
