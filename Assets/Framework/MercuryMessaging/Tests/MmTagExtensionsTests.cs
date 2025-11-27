// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// Tests for DSL Phase 2.6: Tag Configuration Extensions

using NUnit.Framework;
using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Protocol.DSL;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Unit tests for MmTagExtensions.
    /// Tests fluent tag configuration on responders.
    /// </summary>
    [TestFixture]
    public class MmTagExtensionsTests
    {
        private GameObject _testGo;
        private MmRelayNode _relay;
        private TestTagResponder _responder;

        private class TestTagResponder : MmBaseResponder { }

        [SetUp]
        public void SetUp()
        {
            _testGo = new GameObject("TestObject");
            _relay = _testGo.AddComponent<MmRelayNode>();
            _responder = _testGo.AddComponent<TestTagResponder>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testGo != null) Object.DestroyImmediate(_testGo);
        }

        #region WithTag Tests

        [Test]
        public void WithTag_SetsTag()
        {
            _responder.WithTag(MmTag.Tag0);
            Assert.AreEqual(MmTag.Tag0, _responder.Tag);
        }

        [Test]
        public void WithTag_ReturnsResponderForChaining()
        {
            var result = _responder.WithTag(MmTag.Tag1);
            Assert.AreSame(_responder, result);
        }

        [Test]
        public void WithTag_NullResponder_ReturnsNull()
        {
            MmBaseResponder nullResponder = null;
            var result = nullResponder.WithTag(MmTag.Tag0);
            Assert.IsNull(result);
        }

        #endregion

        #region WithTags Tests

        [Test]
        public void WithTags_CombinesTags()
        {
            _responder.WithTags(MmTag.Tag0, MmTag.Tag1);

            // Should have both tags
            Assert.IsTrue((_responder.Tag & MmTag.Tag0) != 0);
            Assert.IsTrue((_responder.Tag & MmTag.Tag1) != 0);
        }

        [Test]
        public void WithTags_ReturnsResponderForChaining()
        {
            var result = _responder.WithTags(MmTag.Tag0, MmTag.Tag1);
            Assert.AreSame(_responder, result);
        }

        #endregion

        #region EnableTagChecking Tests

        [Test]
        public void EnableTagChecking_SetsFlag()
        {
            _responder.TagCheckEnabled = false;
            _responder.EnableTagChecking();
            Assert.IsTrue(_responder.TagCheckEnabled);
        }

        [Test]
        public void DisableTagChecking_ClearsFlag()
        {
            _responder.TagCheckEnabled = true;
            _responder.DisableTagChecking();
            Assert.IsFalse(_responder.TagCheckEnabled);
        }

        #endregion

        #region AddTag/RemoveTag Tests

        [Test]
        public void AddTag_AddsToExisting()
        {
            _responder.Tag = MmTag.Tag0;
            _responder.AddTag(MmTag.Tag1);

            Assert.IsTrue((_responder.Tag & MmTag.Tag0) != 0);
            Assert.IsTrue((_responder.Tag & MmTag.Tag1) != 0);
        }

        [Test]
        public void RemoveTag_RemovesFromExisting()
        {
            _responder.Tag = MmTag.Tag0 | MmTag.Tag1;
            _responder.RemoveTag(MmTag.Tag0);

            Assert.IsFalse((_responder.Tag & MmTag.Tag0) != 0);
            Assert.IsTrue((_responder.Tag & MmTag.Tag1) != 0);
        }

        [Test]
        public void ClearTags_RemovesAllTags()
        {
            _responder.Tag = MmTag.Tag0 | MmTag.Tag1 | MmTag.Tag2;
            _responder.ClearTags();
            Assert.AreEqual(MmTag.Nothing, _responder.Tag);
        }

        #endregion

        #region HasTag Tests

        [Test]
        public void HasTag_ReturnsTrueWhenHasTag()
        {
            _responder.Tag = MmTag.Tag0;
            Assert.IsTrue(_responder.HasTag(MmTag.Tag0));
        }

        [Test]
        public void HasTag_ReturnsFalseWhenNoTag()
        {
            _responder.Tag = MmTag.Tag1;
            Assert.IsFalse(_responder.HasTag(MmTag.Tag0));
        }

        [Test]
        public void HasAllTags_ReturnsTrueWhenAllPresent()
        {
            _responder.Tag = MmTag.Tag0 | MmTag.Tag1 | MmTag.Tag2;
            Assert.IsTrue(_responder.HasAllTags(MmTag.Tag0, MmTag.Tag1));
        }

        [Test]
        public void HasAllTags_ReturnsFalseWhenMissing()
        {
            _responder.Tag = MmTag.Tag0;
            Assert.IsFalse(_responder.HasAllTags(MmTag.Tag0, MmTag.Tag1));
        }

        [Test]
        public void HasAnyTag_ReturnsTrueWhenAnyPresent()
        {
            _responder.Tag = MmTag.Tag0;
            Assert.IsTrue(_responder.HasAnyTag(MmTag.Tag0, MmTag.Tag1));
        }

        [Test]
        public void HasAnyTag_ReturnsFalseWhenNonePresent()
        {
            _responder.Tag = MmTag.Tag2;
            Assert.IsFalse(_responder.HasAnyTag(MmTag.Tag0, MmTag.Tag1));
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void FluentChaining_WorksCorrectly()
        {
            _responder
                .WithTag(MmTag.Tag0)
                .AddTag(MmTag.Tag1)
                .EnableTagChecking();

            Assert.IsTrue(_responder.HasTag(MmTag.Tag0));
            Assert.IsTrue(_responder.HasTag(MmTag.Tag1));
            Assert.IsTrue(_responder.TagCheckEnabled);
        }

        #endregion

        #region ConfigureTags Builder Tests

        [Test]
        public void ConfigureTags_ReturnsBuilder()
        {
            var builder = _relay.ConfigureTags();
            Assert.IsNotNull(builder);
        }

        [Test]
        public void ConfigureTags_ApplyToSelf_SetsTags()
        {
            _relay.ConfigureTags()
                .ApplyToSelf(MmTag.Tag0)
                .Build();

            Assert.AreEqual(MmTag.Tag0, _responder.Tag);
        }

        [Test]
        public void ConfigureTags_ApplyToSelf_EnablesChecking()
        {
            _responder.TagCheckEnabled = false;

            _relay.ConfigureTags()
                .ApplyToSelf(MmTag.Tag0, enableChecking: true)
                .Build();

            Assert.IsTrue(_responder.TagCheckEnabled);
        }

        [Test]
        public void ConfigureTags_Build_ReturnsRelay()
        {
            var result = _relay.ConfigureTags()
                .ApplyToSelf(MmTag.Tag0)
                .Build();

            Assert.AreSame(_relay, result);
        }

        #endregion
    }
}
