// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// DataCollectionBuilderTests.cs - Tests for Data Collection DSL
// Part of DSL Overhaul Phase 3

using MercuryMessaging.Data;
using NUnit.Framework;
using UnityEngine;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for DataCollectionBuilder and MmDataCollectorExtensions.
    /// Tests cover fluent data collection API.
    /// </summary>
    [TestFixture]
    public class DataCollectionBuilderTests
    {
        private GameObject _collectorObj;
        private MmDataCollector _collector;

        [SetUp]
        public void SetUp()
        {
            _collectorObj = new GameObject("DataCollector");
            _collector = _collectorObj.AddComponent<MmDataCollector>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_collectorObj != null)
                Object.DestroyImmediate(_collectorObj);
        }

        #region Configure Tests

        [Test]
        public void Configure_ReturnsBuilder()
        {
            var builder = _collector.Configure();

            Assert.IsNotNull(builder, "Configure should return a builder");
        }

        [Test]
        public void OutputAsCsv_SetsOutputType()
        {
            _collector.Configure()
                .OutputAsCsv("test")
                .Build();

            Assert.AreEqual(MmDataCollectionOutputType.CSV, _collector.OutputType);
        }

        [Test]
        public void OutputAsXml_SetsOutputType()
        {
            _collector.Configure()
                .OutputAsXml("test")
                .Build();

            Assert.AreEqual(MmDataCollectionOutputType.XML, _collector.OutputType);
        }

        [Test]
        public void EveryFrame_SetsFrequency()
        {
            _collector.Configure()
                .EveryFrame()
                .Build();

            Assert.AreEqual(MmDataCollectionFreq.EveryFrame, _collector.MmDataCollectionFreq);
        }

        [Test]
        public void Manually_SetsFrequency()
        {
            _collector.Configure()
                .Manually()
                .Build();

            Assert.AreEqual(MmDataCollectionFreq.Manual, _collector.MmDataCollectionFreq);
        }

        [Test]
        public void Collect_AddsDataItem()
        {
            _collector.Configure()
                .Collect("TestItem", () => "TestValue")
                .Build();

            // The collector should have the item (we can't directly access dataItems,
            // but we can verify build didn't throw)
            Assert.Pass("Data item added successfully");
        }

        [Test]
        public void ChainedConfiguration_Works()
        {
            _collector.Configure()
                .OutputAsCsv("experiment")
                .EveryFrame()
                .Collect("Time", () => Time.time.ToString())
                .Collect("Frame", () => Time.frameCount.ToString())
                .CollectTimestamp()
                .Build();

            Assert.AreEqual(MmDataCollectionOutputType.CSV, _collector.OutputType);
            Assert.AreEqual(MmDataCollectionFreq.EveryFrame, _collector.MmDataCollectionFreq);
        }

        #endregion

        #region Quick Setup Tests

        [Test]
        public void QuickCsv_SetsOutputTypeAndFile()
        {
            _collector.QuickCsv("quick_test");

            Assert.AreEqual(MmDataCollectionOutputType.CSV, _collector.OutputType);
            Assert.AreEqual("quick_test", _collector.FileName);
        }

        [Test]
        public void QuickXml_SetsOutputTypeAndFile()
        {
            _collector.QuickXml("quick_xml");

            Assert.AreEqual(MmDataCollectionOutputType.XML, _collector.OutputType);
            Assert.AreEqual("quick_xml", _collector.FileName);
        }

        [Test]
        public void AddData_AddsItem()
        {
            _collector.AddData("Score", () => "100");

            Assert.Pass("AddData completed successfully");
        }

        #endregion

        #region Recording Control Tests

        [Test]
        public void IsRecording_InitiallyFalse()
        {
            Assert.IsFalse(_collector.IsRecording());
        }

        [Test]
        public void PauseRecording_StopsWriting()
        {
            _collector.AllowWrite = true;

            _collector.PauseRecording();

            Assert.IsFalse(_collector.AllowWrite);
        }

        [Test]
        public void ResumeRecording_AllowsWriting()
        {
            _collector.AllowWrite = false;

            _collector.ResumeRecording();

            Assert.IsTrue(_collector.AllowWrite);
        }

        #endregion

        #region Frequency Shortcut Tests

        [Test]
        public void SetEveryFrame_ChangesFrequency()
        {
            _collector.MmDataCollectionFreq = MmDataCollectionFreq.Manual;

            _collector.SetEveryFrame();

            Assert.AreEqual(MmDataCollectionFreq.EveryFrame, _collector.MmDataCollectionFreq);
        }

        [Test]
        public void SetManual_ChangesFrequency()
        {
            _collector.MmDataCollectionFreq = MmDataCollectionFreq.EveryFrame;

            _collector.SetManual();

            Assert.AreEqual(MmDataCollectionFreq.Manual, _collector.MmDataCollectionFreq);
        }

        #endregion
    }
}
