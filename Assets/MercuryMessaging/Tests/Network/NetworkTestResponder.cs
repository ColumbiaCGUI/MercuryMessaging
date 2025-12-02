// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Test responder for verifying hierarchical message routing over network.
// Logs received messages and tracks counts for verification.

using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Tests.Network
{
    /// <summary>
    /// Test responder that logs all received messages.
    /// Used to verify hierarchical routing works correctly over the network.
    /// </summary>
    public class NetworkTestResponder : MmBaseResponder
    {
        [Header("Test Tracking")]
        [SerializeField] private int messagesReceived;
        [SerializeField] private string lastMessageType;
        [SerializeField] private string lastMessageValue;

        /// <summary>Number of messages received by this responder.</summary>
        public int MessagesReceived => messagesReceived;

        /// <summary>Last message type received.</summary>
        public string LastMessageType => lastMessageType;

        /// <summary>Last message value received.</summary>
        public string LastMessageValue => lastMessageValue;

        /// <summary>
        /// Static counter across all instances for easy verification.
        /// Reset this before each test.
        /// </summary>
        public static int TotalMessagesReceived { get; private set; }

        /// <summary>Reset the static counter before a test.</summary>
        public static void ResetTotalCount() => TotalMessagesReceived = 0;

        protected override void ReceivedMessage(MmMessageString message)
        {
            messagesReceived++;
            TotalMessagesReceived++;
            lastMessageType = "MmString";
            lastMessageValue = message.value;

            Debug.Log($"[NetworkTestResponder] {gameObject.name} received MmString: \"{message.value}\" " +
                      $"(total: {messagesReceived}, global: {TotalMessagesReceived})");
        }

        protected override void ReceivedMessage(MmMessageInt message)
        {
            messagesReceived++;
            TotalMessagesReceived++;
            lastMessageType = "MmInt";
            lastMessageValue = message.value.ToString();

            Debug.Log($"[NetworkTestResponder] {gameObject.name} received MmInt: {message.value} " +
                      $"(total: {messagesReceived}, global: {TotalMessagesReceived})");
        }

        protected override void ReceivedMessage(MmMessageFloat message)
        {
            messagesReceived++;
            TotalMessagesReceived++;
            lastMessageType = "MmFloat";
            lastMessageValue = message.value.ToString("F2");

            Debug.Log($"[NetworkTestResponder] {gameObject.name} received MmFloat: {message.value:F2} " +
                      $"(total: {messagesReceived}, global: {TotalMessagesReceived})");
        }

        protected override void ReceivedMessage(MmMessageBool message)
        {
            messagesReceived++;
            TotalMessagesReceived++;
            lastMessageType = "MmBool";
            lastMessageValue = message.value.ToString();

            Debug.Log($"[NetworkTestResponder] {gameObject.name} received MmBool: {message.value} " +
                      $"(total: {messagesReceived}, global: {TotalMessagesReceived})");
        }

        protected override void ReceivedMessage(MmMessageVector3 message)
        {
            messagesReceived++;
            TotalMessagesReceived++;
            lastMessageType = "MmVector3";
            lastMessageValue = message.value.ToString();

            Debug.Log($"[NetworkTestResponder] {gameObject.name} received MmVector3: {message.value} " +
                      $"(total: {messagesReceived}, global: {TotalMessagesReceived})");
        }

        public override void Initialize()
        {
            base.Initialize();
            messagesReceived++;
            TotalMessagesReceived++;
            lastMessageType = "Initialize";
            lastMessageValue = "";

            Debug.Log($"[NetworkTestResponder] {gameObject.name} received Initialize " +
                      $"(total: {messagesReceived}, global: {TotalMessagesReceived})");
        }

        public override void Refresh(List<MmTransform> transformList)
        {
            base.Refresh(transformList);
            messagesReceived++;
            TotalMessagesReceived++;
            lastMessageType = "Refresh";
            lastMessageValue = "";

            Debug.Log($"[NetworkTestResponder] {gameObject.name} received Refresh " +
                      $"(total: {messagesReceived}, global: {TotalMessagesReceived})");
        }

        public override void SetActive(bool active)
        {
            base.SetActive(active);
            messagesReceived++;
            TotalMessagesReceived++;
            lastMessageType = "SetActive";
            lastMessageValue = active.ToString();

            Debug.Log($"[NetworkTestResponder] {gameObject.name} received SetActive: {active} " +
                      $"(total: {messagesReceived}, global: {TotalMessagesReceived})");
        }

        /// <summary>Reset this responder's counters.</summary>
        public void ResetCounters()
        {
            messagesReceived = 0;
            lastMessageType = "";
            lastMessageValue = "";
        }
    }
}
