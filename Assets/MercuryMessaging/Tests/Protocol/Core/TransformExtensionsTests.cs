// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

using NUnit.Framework;
using UnityEngine;

namespace MercuryMessaging.Tests
{
    [TestFixture]
    public class TransformExtensionsTests
    {
        [Test]
        public void Vector2_ToCSV_DefaultDelimiter()
        {
            // Arrange
            var v = new Vector2(1.5f, 2.5f);

            // Act
            var result = v.ToCSV();

            // Assert
            Assert.AreEqual("1.5000,2.5000", result);
        }

        [Test]
        public void Vector2_ToCSV_CustomDelimiter()
        {
            // Arrange
            var v = new Vector2(3.0f, 4.0f);

            // Act
            var result = v.ToCSV(';');

            // Assert
            Assert.AreEqual("3.0000;4.0000", result);
        }

        [Test]
        public void Vector2_ToCSV_HasTwoComponents()
        {
            // Arrange
            var v = new Vector2(1f, 2f);

            // Act
            var result = v.ToCSV();
            var parts = result.Split(',');

            // Assert
            Assert.AreEqual(2, parts.Length);
        }

        [Test]
        public void Vector3_ToCSV_StillWorks()
        {
            // Arrange - regression test: Vector3.ToCSV must still produce 3 values
            var v = new Vector3(1f, 2f, 3f);

            // Act
            var result = v.ToCSV();

            // Assert
            Assert.AreEqual("1.0000,2.0000,3.0000", result);
        }
    }
}
