// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MercuryMessaging Network Backend Test Runner
// Tests all 13 message types through MmLoopbackBackend and MmBinarySerializer
//

using System.Collections.Generic;
using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Network;

/// <summary>
/// Runtime test runner for MercuryMessaging network backends.
/// Attach to a GameObject in a test scene to verify serialization and message routing.
///
/// Tests:
/// 1. All 13 message types serialize/deserialize correctly
/// 2. Loopback backend echo mode works
/// 3. Connection events fire correctly
///
/// Usage:
/// 1. Create empty scene
/// 2. Add empty GameObject
/// 3. Attach this script
/// 4. Enter Play mode
/// 5. Press buttons to run tests
/// </summary>
public class NetworkBackendTestRunner : MonoBehaviour
{
    [Header("Test Configuration")]
    [Tooltip("Automatically run all tests on Start")]
    public bool runOnStart = true;

    [Tooltip("Enable detailed logging")]
    public bool verboseLogging = true;

    [Header("Test Results")]
    [SerializeField] private int testsPassed;
    [SerializeField] private int testsFailed;
    [SerializeField] private List<string> failedTests = new List<string>();

    private MmLoopbackBackend _backend;
    private List<MmMessage> _receivedMessages = new List<MmMessage>();

    #region Unity Lifecycle

    private void Start()
    {
        if (runOnStart)
        {
            RunAllTests();
        }
    }

    private void OnDestroy()
    {
        _backend?.Shutdown();
    }

    #endregion

    #region Test Runner

    /// <summary>
    /// Run all network backend tests.
    /// </summary>
    public void RunAllTests()
    {
        testsPassed = 0;
        testsFailed = 0;
        failedTests.Clear();
        _receivedMessages.Clear();

        Log("=== MercuryMessaging Network Backend Tests ===");
        Log("");

        // Initialize backend
        InitializeBackend();

        // Test all message types
        TestMmVoid();
        TestMmInt();
        TestMmBool();
        TestMmFloat();
        TestMmString();
        TestMmVector3();
        TestMmVector4();
        TestMmQuaternion();
        TestMmByteArray();
        TestMmTransform();
        TestMmTransformList();
        TestMmGameObject();

        // Test connection events
        TestConnectionEvents();

        // Summary
        Log("");
        Log("=== Test Summary ===");
        Log($"Passed: {testsPassed}");
        Log($"Failed: {testsFailed}");

        if (testsFailed > 0)
        {
            Log("Failed tests:");
            foreach (var test in failedTests)
            {
                Log($"  - {test}");
            }
        }
        else
        {
            Log("All tests passed!");
        }

        // Cleanup
        _backend.Shutdown();
    }

    private void InitializeBackend()
    {
        _backend = new MmLoopbackBackend
        {
            Mode = MmLoopbackBackend.LoopbackMode.Echo,
            UseMessageQueue = false
        };

        _backend.OnMessageReceived += OnMessageReceived;
        _backend.Initialize();

        Log("Backend initialized: " + _backend.BackendName);
        Log($"  IsConnected: {_backend.IsConnected}");
        Log($"  IsServer: {_backend.IsServer}");
        Log($"  IsClient: {_backend.IsClient}");
        Log("");
    }

    private void OnMessageReceived(byte[] data, int senderId)
    {
        try
        {
            var message = MmBinarySerializer.Deserialize(data);
            _receivedMessages.Add(message);

            if (verboseLogging)
            {
                Log($"  Received: {message.MmMessageType} from sender {senderId}");
            }
        }
        catch (System.Exception e)
        {
            LogError($"  Failed to deserialize: {e}");
        }
    }

    #endregion

    #region Message Type Tests

    private void TestMmVoid()
    {
        string testName = "MmVoid";
        Log($"Testing {testName}...");
        _receivedMessages.Clear();

        var original = new MmMessage(MmMetadataBlockHelper.Default, MmMessageType.MmVoid);
        original.MmMethod = MmMethod.Initialize;

        if (SendAndVerify(original, testName))
        {
            var received = _receivedMessages[0];
            Assert(received.MmMethod == MmMethod.Initialize, testName, "Method mismatch");
        }
    }

    private void TestMmInt()
    {
        string testName = "MmInt";
        Log($"Testing {testName}...");
        _receivedMessages.Clear();

        var original = new MmMessageInt(42, MmMethod.MessageInt, MmMetadataBlockHelper.Default);

        if (SendAndVerify(original, testName))
        {
            var received = (MmMessageInt)_receivedMessages[0];
            Assert(received.value == 42, testName, $"Value mismatch: expected 42, got {received.value}");
        }
    }

    private void TestMmBool()
    {
        string testName = "MmBool";
        Log($"Testing {testName}...");
        _receivedMessages.Clear();

        var original = new MmMessageBool(true, MmMethod.SetActive, MmMetadataBlockHelper.Default);

        if (SendAndVerify(original, testName))
        {
            var received = (MmMessageBool)_receivedMessages[0];
            Assert(received.value == true, testName, $"Value mismatch: expected true, got {received.value}");
        }
    }

    private void TestMmFloat()
    {
        string testName = "MmFloat";
        Log($"Testing {testName}...");
        _receivedMessages.Clear();

        var original = new MmMessageFloat(3.14159f, MmMethod.MessageFloat, MmMetadataBlockHelper.Default);

        if (SendAndVerify(original, testName))
        {
            var received = (MmMessageFloat)_receivedMessages[0];
            Assert(Mathf.Approximately(received.value, 3.14159f), testName,
                $"Value mismatch: expected 3.14159, got {received.value}");
        }
    }

    private void TestMmString()
    {
        string testName = "MmString";
        Log($"Testing {testName}...");
        _receivedMessages.Clear();

        var original = new MmMessageString("Hello Network!", MmMethod.MessageString, MmMetadataBlockHelper.Default);

        if (SendAndVerify(original, testName))
        {
            var received = (MmMessageString)_receivedMessages[0];
            Assert(received.value == "Hello Network!", testName,
                $"Value mismatch: expected 'Hello Network!', got '{received.value}'");
        }
    }

    private void TestMmVector3()
    {
        string testName = "MmVector3";
        Log($"Testing {testName}...");
        _receivedMessages.Clear();

        var original = new MmMessageVector3(new Vector3(1.5f, 2.5f, 3.5f),
            MmMethod.MessageVector3, MmMetadataBlockHelper.Default);

        if (SendAndVerify(original, testName))
        {
            var received = (MmMessageVector3)_receivedMessages[0];
            Assert(received.value == new Vector3(1.5f, 2.5f, 3.5f), testName,
                $"Value mismatch: expected (1.5, 2.5, 3.5), got {received.value}");
        }
    }

    private void TestMmVector4()
    {
        string testName = "MmVector4";
        Log($"Testing {testName}...");
        _receivedMessages.Clear();

        var original = new MmMessageVector4(new Vector4(1f, 2f, 3f, 4f),
            MmMethod.MessageVector4, MmMetadataBlockHelper.Default);

        if (SendAndVerify(original, testName))
        {
            var received = (MmMessageVector4)_receivedMessages[0];
            Assert(received.value == new Vector4(1f, 2f, 3f, 4f), testName,
                $"Value mismatch: expected (1, 2, 3, 4), got {received.value}");
        }
    }

    private void TestMmQuaternion()
    {
        string testName = "MmQuaternion";
        Log($"Testing {testName}...");
        _receivedMessages.Clear();

        var rotation = Quaternion.Euler(45f, 90f, 0f);
        var original = new MmMessageQuaternion(rotation, MmMethod.MessageQuaternion, MmMetadataBlockHelper.Default);

        if (SendAndVerify(original, testName))
        {
            var received = (MmMessageQuaternion)_receivedMessages[0];
            // Compare with tolerance due to floating point
            float dot = Quaternion.Dot(received.value, rotation);
            Assert(Mathf.Abs(dot) > 0.9999f, testName,
                $"Rotation mismatch: dot product = {dot}");
        }
    }

    private void TestMmByteArray()
    {
        string testName = "MmByteArray";
        Log($"Testing {testName}...");
        _receivedMessages.Clear();

        byte[] data = { 0x01, 0x02, 0x03, 0xFF, 0xFE };
        var original = new MmMessageByteArray(data, MmMethod.MessageByteArray, MmMetadataBlockHelper.Default);

        if (SendAndVerify(original, testName))
        {
            var received = (MmMessageByteArray)_receivedMessages[0];
            bool match = received.byteArr.Length == data.Length;
            if (match)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (received.byteArr[i] != data[i])
                    {
                        match = false;
                        break;
                    }
                }
            }
            Assert(match, testName, "Byte array content mismatch");
        }
    }

    private void TestMmTransform()
    {
        string testName = "MmTransform";
        Log($"Testing {testName}...");
        _receivedMessages.Clear();

        var transform = new MmTransform(
            new Vector3(10f, 20f, 30f),  // Translation
            new Vector3(1f, 2f, 3f),      // Scale
            Quaternion.Euler(0f, 90f, 0f) // Rotation
        );
        var original = new MmMessageTransform(transform, MmMethod.MessageTransform, MmMetadataBlockHelper.Default);

        if (SendAndVerify(original, testName))
        {
            var received = (MmMessageTransform)_receivedMessages[0];
            Assert(received.MmTransform.Translation == transform.Translation, testName,
                $"Translation mismatch: expected {transform.Translation}, got {received.MmTransform.Translation}");
            Assert(received.MmTransform.Scale == transform.Scale, testName,
                $"Scale mismatch: expected {transform.Scale}, got {received.MmTransform.Scale}");
        }
    }

    private void TestMmTransformList()
    {
        string testName = "MmTransformList";
        Log($"Testing {testName}...");
        _receivedMessages.Clear();

        var transforms = new List<MmTransform>
        {
            new MmTransform(Vector3.zero, Vector3.one, Quaternion.identity),
            new MmTransform(Vector3.one, Vector3.one * 2f, Quaternion.Euler(0, 45, 0))
        };
        var original = new MmMessageTransformList(transforms, MmMethod.MessageTransformList, MmMetadataBlockHelper.Default);

        if (SendAndVerify(original, testName))
        {
            var received = (MmMessageTransformList)_receivedMessages[0];
            Assert(received.transforms.Count == 2, testName,
                $"List count mismatch: expected 2, got {received.transforms.Count}");
        }
    }

    private void TestMmGameObject()
    {
        string testName = "MmGameObject";
        Log($"Testing {testName}...");
        _receivedMessages.Clear();

        // Create a message with a network ID (actual GameObject resolution requires IMmGameObjectResolver)
        var original = new MmMessageGameObject(null, MmMethod.MessageGameObject, MmMetadataBlockHelper.Default);
        original.GameObjectNetId = 12345;

        if (SendAndVerify(original, testName))
        {
            var received = (MmMessageGameObject)_receivedMessages[0];
            Assert(received.GameObjectNetId == 12345, testName,
                $"NetId mismatch: expected 12345, got {received.GameObjectNetId}");
        }
    }

    private void TestConnectionEvents()
    {
        string testName = "ConnectionEvents";
        Log($"Testing {testName}...");

        bool connectedFired = false;
        bool disconnectedFired = false;

        var testBackend = new MmLoopbackBackend { Mode = MmLoopbackBackend.LoopbackMode.Client };
        testBackend.OnConnectedToServer += () => connectedFired = true;
        testBackend.OnDisconnectedFromServer += () => disconnectedFired = true;

        testBackend.Initialize();
        testBackend.Shutdown();

        Assert(connectedFired, testName, "OnConnectedToServer not fired");
        Assert(disconnectedFired, testName, "OnDisconnectedFromServer not fired");
    }

    #endregion

    #region Helpers

    private bool SendAndVerify(MmMessage original, string testName)
    {
        try
        {
            // Serialize
            byte[] data = MmBinarySerializer.Serialize(original);

            if (verboseLogging)
            {
                Log($"  Serialized {original.MmMessageType}: {data.Length} bytes");
            }

            // Send through loopback (will trigger OnMessageReceived)
            _backend.SendToServer(data);

            // Verify we received a message
            if (_receivedMessages.Count == 0)
            {
                Fail(testName, "No message received");
                return false;
            }

            // Verify message type matches
            if (_receivedMessages[0].MmMessageType != original.MmMessageType)
            {
                Fail(testName, $"Type mismatch: expected {original.MmMessageType}, got {_receivedMessages[0].MmMessageType}");
                return false;
            }

            return true;
        }
        catch (System.Exception e)
        {
            Fail(testName, $"Exception: {e}");
            return false;
        }
    }

    private void Assert(bool condition, string testName, string message)
    {
        if (condition)
        {
            Pass(testName);
        }
        else
        {
            Fail(testName, message);
        }
    }

    private void Pass(string testName)
    {
        testsPassed++;
        Log($"  PASS: {testName}");
    }

    private void Fail(string testName, string reason)
    {
        testsFailed++;
        failedTests.Add($"{testName}: {reason}");
        LogError($"  FAIL: {testName} - {reason}");
    }

    private void Log(string message)
    {
        Debug.Log($"[NetworkTest] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[NetworkTest] {message}");
    }

    #endregion

    #region Editor GUI

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("MercuryMessaging Network Tests", GUI.skin.box);

        if (GUILayout.Button("Run All Tests"))
        {
            RunAllTests();
        }

        GUILayout.Space(10);
        GUILayout.Label($"Passed: {testsPassed}");
        GUILayout.Label($"Failed: {testsFailed}");

        if (testsFailed > 0)
        {
            GUILayout.Label("Failed Tests:", GUI.skin.box);
            foreach (var test in failedTests)
            {
                GUILayout.Label($"  {test}");
            }
        }

        GUILayout.EndArea();
    }

    #endregion
}
