// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MercuryMessaging.Task;

namespace MercuryMessaging.Tests
{
    [TestFixture]
    public class MmTaskInfoTests
    {
        private bool _previousIgnoreFailingMessages;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _previousIgnoreFailingMessages = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            LogAssert.ignoreFailingMessages = _previousIgnoreFailingMessages;
        }

        [Test]
        public void Parse_ValidInput_SetsAllFields()
        {
            // Arrange
            var taskInfo = new MmTaskInfo();

            // Act
            taskInfo.Parse("1,2,3,4,False,TestTask");

            // Assert
            Assert.AreEqual(1, taskInfo.RecordId);
            Assert.AreEqual(2, taskInfo.UserId);
            Assert.AreEqual(3, taskInfo.UserSequence);
            Assert.AreEqual(4, taskInfo.TaskId);
            Assert.AreEqual(false, taskInfo.DoNotRecordData);
            Assert.AreEqual("TestTask", taskInfo.TaskName);
        }

        [Test]
        public void Parse_InvalidInt_DoesNotThrow()
        {
            // Arrange
            var taskInfo = new MmTaskInfo();

            // Act & Assert - should not throw, logs warning and defaults to 0
            Assert.DoesNotThrow(() => taskInfo.Parse("abc,2,3,4,False,TestTask"));
            Assert.AreEqual(0, taskInfo.RecordId);
        }

        [Test]
        public void Parse_InvalidBool_DoesNotThrow()
        {
            // Arrange
            var taskInfo = new MmTaskInfo();

            // Act & Assert - should not throw, logs warning and defaults to false
            Assert.DoesNotThrow(() => taskInfo.Parse("1,2,3,4,notabool,TestTask"));
        }

        [Test]
        public void Parse_ReturnsWordCount6()
        {
            // Arrange
            var taskInfo = new MmTaskInfo();

            // Act
            var count = taskInfo.Parse("1,2,3,4,True,TaskA");

            // Assert
            Assert.AreEqual(6, count);
        }
    }
}
