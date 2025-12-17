// Tutorial 12: VR Experiment - Data Logger
// Logs experiment events using MercuryMessaging's data collection system.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MercuryMessaging.Examples.Tutorial12
{
    /// <summary>
    /// Logs experiment events to CSV file using MercuryMessaging message handling.
    /// Demonstrates using MmBaseResponder for data collection in experiments.
    /// </summary>
    public class T12_DataLogger : MmBaseResponder
    {
        [Header("Logging Configuration")]
        [SerializeField] private string participantId = "P001";
        [SerializeField] private string sessionId = "S001";
        [SerializeField] private bool logToConsole = true;
        [SerializeField] private bool logToFile = true;

        [Header("File Output")]
        [SerializeField] private string outputFolder = "ExperimentData";

        private List<LogEntry> _logEntries = new List<LogEntry>();
        private string _outputPath;
        private float _experimentStartTime;

        public struct LogEntry
        {
            public float Timestamp;
            public string EventType;
            public string EventData;
            public string ParticipantId;
            public string SessionId;
        }

        public override void Awake()
        {
            base.Awake();

            // Create output folder
            _outputPath = Path.Combine(Application.dataPath, "..", outputFolder);
            if (logToFile && !Directory.Exists(_outputPath))
            {
                Directory.CreateDirectory(_outputPath);
            }
        }

        protected override void ReceivedMessage(MmMessageString message)
        {
            string eventType = message.value;

            // Handle special events
            switch (eventType)
            {
                case "ExperimentStart":
                    _experimentStartTime = Time.time;
                    LogEvent("EXPERIMENT_START", "");
                    break;

                case "ExperimentEnd":
                    LogEvent("EXPERIMENT_END", "");
                    SaveLogToFile();
                    break;

                case "GoStimulus":
                    LogEvent("STIMULUS_ONSET", "type=GO");
                    break;

                case "NoGoStimulus":
                    LogEvent("STIMULUS_ONSET", "type=NO_GO");
                    break;

                case "Response":
                    LogEvent("RESPONSE", "");
                    break;

                default:
                    LogEvent("MESSAGE", eventType);
                    break;
            }
        }

        protected override void ReceivedMessage(MmMessageInt message)
        {
            LogEvent("INT_MESSAGE", $"value={message.value}");
        }

        protected override void ReceivedMessage(MmMessageFloat message)
        {
            LogEvent("FLOAT_MESSAGE", $"value={message.value:F3}");
        }

        private void LogEvent(string eventType, string eventData)
        {
            float timestamp = Time.time - _experimentStartTime;

            var entry = new LogEntry
            {
                Timestamp = timestamp,
                EventType = eventType,
                EventData = eventData,
                ParticipantId = participantId,
                SessionId = sessionId
            };

            _logEntries.Add(entry);

            if (logToConsole)
            {
                Debug.Log($"[T12_Log] {timestamp:F3}s | {eventType} | {eventData}");
            }
        }

        private void SaveLogToFile()
        {
            if (!logToFile) return;

            string filename = $"exp_{participantId}_{sessionId}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string filepath = Path.Combine(_outputPath, filename);

            try
            {
                using (var writer = new StreamWriter(filepath))
                {
                    // Header
                    writer.WriteLine("timestamp_s,event_type,event_data,participant_id,session_id");

                    // Data
                    foreach (var entry in _logEntries)
                    {
                        writer.WriteLine($"{entry.Timestamp:F3},{entry.EventType},{entry.EventData},{entry.ParticipantId},{entry.SessionId}");
                    }
                }

                Debug.Log($"[T12_Log] Data saved to: {filepath}");
                Debug.Log($"[T12_Log] Total events logged: {_logEntries.Count}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[T12_Log] Failed to save log file: {e.Message}");
            }
        }

        private void OnApplicationQuit()
        {
            // Save any unsaved data on quit
            if (_logEntries.Count > 0 && logToFile)
            {
                SaveLogToFile();
            }
        }
    }
}
