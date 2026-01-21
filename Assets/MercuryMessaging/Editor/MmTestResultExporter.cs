// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmTestResultExporter.cs - Automatic test result export for CI/CD and AI assistants
// Part of DX3: Developer Experience Improvements

using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace MercuryMessaging.Editor
{
    /// <summary>
    /// Automatically exports test results to XML after test runs complete.
    /// Results are saved to dev/test-results/ with timestamps.
    /// </summary>
    [InitializeOnLoad]
    public class MmTestResultExporter : ICallbacks
    {
        private static readonly string OutputDirectory = "dev/test-results";
        private static readonly string OutputPrefix = "TestResults_";

        static MmTestResultExporter()
        {
            // Register callback on editor load
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new MmTestResultExporter());
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            Debug.Log($"[MmTestResultExporter] Test run started: {testsToRun.Name}");
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            try
            {
                ExportResults(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MmTestResultExporter] Failed to export results: {e.Message}");
            }
        }

        public void TestStarted(ITestAdaptor test) { }

        public void TestFinished(ITestResultAdaptor result) { }

        private void ExportResults(ITestResultAdaptor result)
        {
            // Ensure output directory exists
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }

            // Generate filename with timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"{OutputPrefix}{timestamp}.xml";
            string fullPath = Path.Combine(OutputDirectory, filename);

            // Export to NUnit XML format
            var xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8
            };

            using (var writer = XmlWriter.Create(fullPath, xmlSettings))
            {
                WriteTestResults(writer, result);
            }

            // Calculate summary
            int passed = CountResults(result, TestStatus.Passed);
            int failed = CountResults(result, TestStatus.Failed);
            int skipped = CountResults(result, TestStatus.Skipped);
            int total = passed + failed + skipped;

            Debug.Log($"[MmTestResultExporter] Results exported to: {fullPath}");
            Debug.Log($"[MmTestResultExporter] Summary: {passed}/{total} passed, {failed} failed, {skipped} skipped");

            // Also write a quick summary file for easy parsing
            WriteSummaryFile(fullPath.Replace(".xml", "_summary.txt"), result, passed, failed, skipped);
        }

        private void WriteTestResults(XmlWriter writer, ITestResultAdaptor result)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("test-run");

            writer.WriteAttributeString("id", "1");
            writer.WriteAttributeString("name", result.Name);
            writer.WriteAttributeString("fullname", result.FullName);
            writer.WriteAttributeString("result", result.ResultState.ToString());
            writer.WriteAttributeString("total", result.PassCount.ToString());
            writer.WriteAttributeString("passed", result.PassCount.ToString());
            writer.WriteAttributeString("failed", result.FailCount.ToString());
            writer.WriteAttributeString("skipped", result.SkipCount.ToString());
            writer.WriteAttributeString("duration", result.Duration.ToString("F3"));
            writer.WriteAttributeString("start-time", result.StartTime.ToString("o"));
            writer.WriteAttributeString("end-time", result.EndTime.ToString("o"));

            WriteTestSuite(writer, result);

            writer.WriteEndElement(); // test-run
            writer.WriteEndDocument();
        }

        private void WriteTestSuite(XmlWriter writer, ITestResultAdaptor result)
        {
            writer.WriteStartElement("test-suite");
            writer.WriteAttributeString("name", result.Name);
            writer.WriteAttributeString("fullname", result.FullName);
            writer.WriteAttributeString("result", result.ResultState.ToString());
            writer.WriteAttributeString("duration", result.Duration.ToString("F3"));

            if (result.HasChildren)
            {
                foreach (var child in result.Children)
                {
                    if (child.HasChildren)
                    {
                        WriteTestSuite(writer, child);
                    }
                    else
                    {
                        WriteTestCase(writer, child);
                    }
                }
            }

            writer.WriteEndElement(); // test-suite
        }

        private void WriteTestCase(XmlWriter writer, ITestResultAdaptor result)
        {
            writer.WriteStartElement("test-case");
            writer.WriteAttributeString("name", result.Name);
            writer.WriteAttributeString("fullname", result.FullName);
            writer.WriteAttributeString("result", result.ResultState.ToString());
            writer.WriteAttributeString("duration", result.Duration.ToString("F3"));

            if (result.TestStatus == TestStatus.Failed)
            {
                writer.WriteStartElement("failure");
                if (!string.IsNullOrEmpty(result.Message))
                {
                    writer.WriteElementString("message", result.Message);
                }
                if (!string.IsNullOrEmpty(result.StackTrace))
                {
                    writer.WriteElementString("stack-trace", result.StackTrace);
                }
                writer.WriteEndElement(); // failure
            }

            if (!string.IsNullOrEmpty(result.Output))
            {
                writer.WriteElementString("output", result.Output);
            }

            writer.WriteEndElement(); // test-case
        }

        private int CountResults(ITestResultAdaptor result, TestStatus status)
        {
            int count = 0;
            CountResultsRecursive(result, status, ref count);
            return count;
        }

        private void CountResultsRecursive(ITestResultAdaptor result, TestStatus status, ref int count)
        {
            if (!result.HasChildren)
            {
                if (result.TestStatus == status)
                    count++;
            }
            else
            {
                foreach (var child in result.Children)
                {
                    CountResultsRecursive(child, status, ref count);
                }
            }
        }

        private void WriteSummaryFile(string path, ITestResultAdaptor result, int passed, int failed, int skipped)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Test Results Summary");
            sb.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Duration: {result.Duration:F2}s");
            sb.AppendLine($"Result: {result.ResultState}");
            sb.AppendLine();
            sb.AppendLine($"## Statistics");
            sb.AppendLine($"- Passed: {passed}");
            sb.AppendLine($"- Failed: {failed}");
            sb.AppendLine($"- Skipped: {skipped}");
            sb.AppendLine($"- Total: {passed + failed + skipped}");

            if (failed > 0)
            {
                sb.AppendLine();
                sb.AppendLine("## Failed Tests");
                WriteFailedTests(sb, result);
            }

            File.WriteAllText(path, sb.ToString());
        }

        private void WriteFailedTests(StringBuilder sb, ITestResultAdaptor result)
        {
            if (!result.HasChildren)
            {
                if (result.TestStatus == TestStatus.Failed)
                {
                    sb.AppendLine($"- {result.FullName}");
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        sb.AppendLine($"  Message: {result.Message.Split('\n')[0]}");
                    }
                }
            }
            else
            {
                foreach (var child in result.Children)
                {
                    WriteFailedTests(sb, child);
                }
            }
        }
    }
}
