// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmDataCollectorExtensions.cs - Fluent DSL extensions for data collection
// Part of DSL Overhaul Phase 3

using System;

namespace MercuryMessaging.Data
{
    /// <summary>
    /// Extension methods for MmDataCollector to enable fluent configuration.
    /// Provides a clean, chainable API for setting up data collection.
    ///
    /// Example Usage:
    /// <code>
    /// // Full fluent configuration
    /// collector.Configure()
    ///     .OutputAsCsv("experiment_data")
    ///     .EveryFrame()
    ///     .Collect("Position", () => transform.position.ToString())
    ///     .CollectTimestamp()
    ///     .Start();
    ///
    /// // Quick setup helpers
    /// collector.QuickCsv("results", Application.persistentDataPath);
    /// collector.AddData("Score", () => score.ToString());
    /// collector.StartRecording();
    ///
    /// // Later...
    /// collector.StopRecording();
    /// </code>
    /// </summary>
    public static class MmDataCollectorExtensions
    {
        #region Fluent Configuration

        /// <summary>
        /// Start configuring the data collector with a fluent builder.
        /// </summary>
        public static DataCollectionBuilder Configure(this MmDataCollector collector)
        {
            if (collector == null)
                throw new ArgumentNullException(nameof(collector));

            return new DataCollectionBuilder(collector);
        }

        #endregion

        #region Quick Setup

        /// <summary>
        /// Quick setup for CSV output.
        /// </summary>
        public static void QuickCsv(this MmDataCollector collector, string fileName, string location = null)
        {
            if (collector == null) return;

            collector.OutputType = MmDataCollectionOutputType.CSV;
            collector.FileName = fileName;
            collector.FileLocation = location ?? UnityEngine.Application.persistentDataPath;
        }

        /// <summary>
        /// Quick setup for XML output.
        /// </summary>
        public static void QuickXml(this MmDataCollector collector, string fileName, string location = null)
        {
            if (collector == null) return;

            collector.OutputType = MmDataCollectionOutputType.XML;
            collector.FileName = fileName;
            collector.FileLocation = location ?? UnityEngine.Application.persistentDataPath;
        }

        /// <summary>
        /// Add a data item using cleaner syntax.
        /// </summary>
        public static void AddData(this MmDataCollector collector, string name, Func<string> dataProvider)
        {
            collector?.Add(name, dataProvider);
        }

        #endregion

        #region Recording Control

        /// <summary>
        /// Start recording data.
        /// </summary>
        public static void StartRecording(this MmDataCollector collector)
        {
            if (collector == null) return;

            if (!collector.AllowWrite)
            {
                collector.CreateDataHandler(collector.FileLocation, collector.FileName);
                collector.OpenTag();
            }
        }

        /// <summary>
        /// Stop recording and close the file.
        /// </summary>
        public static void StopRecording(this MmDataCollector collector)
        {
            if (collector == null || !collector.AllowWrite) return;

            collector.CloseTag();
            collector.CloseDataHandler();
            collector.AllowWrite = false;
        }

        /// <summary>
        /// Pause recording (keeps file open but stops writing).
        /// </summary>
        public static void PauseRecording(this MmDataCollector collector)
        {
            if (collector != null)
                collector.AllowWrite = false;
        }

        /// <summary>
        /// Resume recording.
        /// </summary>
        public static void ResumeRecording(this MmDataCollector collector)
        {
            if (collector != null)
                collector.AllowWrite = true;
        }

        /// <summary>
        /// Check if currently recording.
        /// </summary>
        public static bool IsRecording(this MmDataCollector collector)
        {
            return collector != null && collector.AllowWrite;
        }

        /// <summary>
        /// Manually write a single data point (for Manual frequency mode).
        /// </summary>
        public static void WriteNow(this MmDataCollector collector)
        {
            if (collector != null && collector.AllowWrite)
                collector.Write();
        }

        #endregion

        #region Frequency Shortcuts

        /// <summary>
        /// Set collection frequency to every frame.
        /// </summary>
        public static void SetEveryFrame(this MmDataCollector collector)
        {
            if (collector != null)
                collector.MmDataCollectionFreq = MmDataCollectionFreq.EveryFrame;
        }

        /// <summary>
        /// Set collection frequency to manual.
        /// </summary>
        public static void SetManual(this MmDataCollector collector)
        {
            if (collector != null)
                collector.MmDataCollectionFreq = MmDataCollectionFreq.Manual;
        }

        /// <summary>
        /// Set collection frequency to end of task.
        /// </summary>
        public static void SetEndOfTask(this MmDataCollector collector)
        {
            if (collector != null)
                collector.MmDataCollectionFreq = MmDataCollectionFreq.EndOfTask;
        }

        #endregion
    }
}
