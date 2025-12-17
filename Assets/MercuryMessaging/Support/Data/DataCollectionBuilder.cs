// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// DataCollectionBuilder.cs - Fluent builder for data collection configuration
// Part of DSL Overhaul Phase 3

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Data
{
    /// <summary>
    /// Fluent builder for configuring MmDataCollector instances.
    /// Provides a clean, chainable API for setting up data collection.
    ///
    /// Example Usage:
    /// <code>
    /// collector.Configure()
    ///     .OutputAsCsv("experiment_results")
    ///     .AtLocation(Application.persistentDataPath)
    ///     .EveryFrame()
    ///     .Collect("Position", () => transform.position.ToString())
    ///     .Collect("Rotation", () => transform.rotation.eulerAngles.ToString())
    ///     .Collect("Time", () => Time.time.ToString("F3"))
    ///     .Start();
    /// </code>
    /// </summary>
    public class DataCollectionBuilder
    {
        private readonly MmDataCollector _collector;
        private readonly List<(string name, Func<string> printData)> _dataItems =
            new List<(string, Func<string>)>();

        private string _fileName = "data";
        private string _fileLocation;
        private MmDataCollectionOutputType _outputType = MmDataCollectionOutputType.CSV;
        private MmDataCollectionFreq _frequency = MmDataCollectionFreq.Manual;

        /// <summary>
        /// Create a new data collection builder.
        /// </summary>
        public DataCollectionBuilder(MmDataCollector collector)
        {
            _collector = collector ?? throw new ArgumentNullException(nameof(collector));
            _fileLocation = Application.persistentDataPath;
        }

        #region Output Format

        /// <summary>
        /// Set output format to CSV with specified filename.
        /// </summary>
        public DataCollectionBuilder OutputAsCsv(string fileName)
        {
            _outputType = MmDataCollectionOutputType.CSV;
            _fileName = fileName ?? "data";
            return this;
        }

        /// <summary>
        /// Set output format to XML with specified filename.
        /// </summary>
        public DataCollectionBuilder OutputAsXml(string fileName)
        {
            _outputType = MmDataCollectionOutputType.XML;
            _fileName = fileName ?? "data";
            return this;
        }

        /// <summary>
        /// Set the output file location.
        /// </summary>
        public DataCollectionBuilder AtLocation(string path)
        {
            _fileLocation = path ?? Application.persistentDataPath;
            return this;
        }

        /// <summary>
        /// Use persistent data path for output.
        /// </summary>
        public DataCollectionBuilder InPersistentData()
        {
            _fileLocation = Application.persistentDataPath;
            return this;
        }

        /// <summary>
        /// Use streaming assets path for output.
        /// </summary>
        public DataCollectionBuilder InStreamingAssets()
        {
            _fileLocation = Application.streamingAssetsPath;
            return this;
        }

        #endregion

        #region Collection Frequency

        /// <summary>
        /// Collect data every frame (in LateUpdate).
        /// </summary>
        public DataCollectionBuilder EveryFrame()
        {
            _frequency = MmDataCollectionFreq.EveryFrame;
            return this;
        }

        /// <summary>
        /// Collect data only when manually triggered.
        /// </summary>
        public DataCollectionBuilder Manually()
        {
            _frequency = MmDataCollectionFreq.Manual;
            return this;
        }

        /// <summary>
        /// Collect data at end of task.
        /// </summary>
        public DataCollectionBuilder AtEndOfTask()
        {
            _frequency = MmDataCollectionFreq.EndOfTask;
            return this;
        }

        #endregion

        #region Data Items

        /// <summary>
        /// Add a data item to collect with a string provider function.
        /// </summary>
        public DataCollectionBuilder Collect(string name, Func<string> dataProvider)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (dataProvider == null)
                throw new ArgumentNullException(nameof(dataProvider));

            _dataItems.Add((name, dataProvider));
            return this;
        }

        /// <summary>
        /// Add a Vector3 data item (auto-converted to string).
        /// </summary>
        public DataCollectionBuilder Collect(string name, Func<Vector3> dataProvider)
        {
            return Collect(name, () =>
            {
                var v = dataProvider();
                return $"{v.x:F4},{v.y:F4},{v.z:F4}";
            });
        }

        /// <summary>
        /// Add a Quaternion data item (auto-converted to euler angles string).
        /// </summary>
        public DataCollectionBuilder Collect(string name, Func<Quaternion> dataProvider)
        {
            return Collect(name, () =>
            {
                var e = dataProvider().eulerAngles;
                return $"{e.x:F2},{e.y:F2},{e.z:F2}";
            });
        }

        /// <summary>
        /// Add a float data item.
        /// </summary>
        public DataCollectionBuilder Collect(string name, Func<float> dataProvider, string format = "F4")
        {
            return Collect(name, () => dataProvider().ToString(format));
        }

        /// <summary>
        /// Add an int data item.
        /// </summary>
        public DataCollectionBuilder Collect(string name, Func<int> dataProvider)
        {
            return Collect(name, () => dataProvider().ToString());
        }

        /// <summary>
        /// Add a bool data item.
        /// </summary>
        public DataCollectionBuilder Collect(string name, Func<bool> dataProvider)
        {
            return Collect(name, () => dataProvider() ? "1" : "0");
        }

        /// <summary>
        /// Add a Transform position data item.
        /// </summary>
        public DataCollectionBuilder CollectPosition(string name, Transform transform)
        {
            return Collect(name, () => transform.position);
        }

        /// <summary>
        /// Add a Transform rotation data item.
        /// </summary>
        public DataCollectionBuilder CollectRotation(string name, Transform transform)
        {
            return Collect(name, () => transform.rotation);
        }

        /// <summary>
        /// Add timestamp data item.
        /// </summary>
        public DataCollectionBuilder CollectTimestamp(string name = "Timestamp")
        {
            return Collect(name, () => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        }

        /// <summary>
        /// Add Unity time data item.
        /// </summary>
        public DataCollectionBuilder CollectGameTime(string name = "GameTime")
        {
            return Collect(name, () => Time.time.ToString("F4"));
        }

        /// <summary>
        /// Add frame count data item.
        /// </summary>
        public DataCollectionBuilder CollectFrameCount(string name = "Frame")
        {
            return Collect(name, () => Time.frameCount.ToString());
        }

        #endregion

        #region Build

        /// <summary>
        /// Apply configuration and prepare for collection (does not start).
        /// </summary>
        public MmDataCollector Build()
        {
            // Apply settings
            _collector.OutputType = _outputType;
            _collector.MmDataCollectionFreq = _frequency;

            // Clear and add items
            _collector.Clear();
            foreach (var (name, printData) in _dataItems)
            {
                _collector.Add(name, printData);
            }

            return _collector;
        }

        /// <summary>
        /// Apply configuration and start data collection.
        /// </summary>
        public MmDataCollector Start()
        {
            Build();

            // Create handler and start
            _collector.CreateDataHandler(_fileLocation, _fileName);
            _collector.OpenTag();

            return _collector;
        }

        #endregion
    }
}
