using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RomanticWeb.Mapping.Conventions;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Mapping.Providers;
using RomanticWeb.Tests.Stubs;

namespace RomanticWeb.Tests.Mapping.Conventions
{
    [TestFixture]
    public class RdfListConventionTests
    {
        private RdfListConvention _rdfListConvention;

        [SetUp]
        public void Setup()
        {
            _rdfListConvention = new RdfListConvention();
        }

        [Test]
        public void Should_be_applied_for_IList()
        {
            // given
            var mapping = new Mock<ICollectionMappingProvider>();
            mapping.SetupGet(instance => instance.PropertyInfo).Returns(new TestPropertyInfo(typeof(IList<int>)));
            mapping.SetupGet(instance => instance.StoreAs).Returns(StoreAs.Undefined);

            // when
            var shouldApply = _rdfListConvention.ShouldApply(mapping.Object);

            // then
            shouldApply.Should().BeTrue();
        }

        [TestCase(typeof(ICollection<int>))]
        [TestCase(typeof(IEnumerable<int>))]
        [TestCase(typeof(ISet<int>))]
        [TestCase(typeof(IDictionary<int, string>))]
        public void Should_not_be_applied_for_non_list_collections(Type collectionType)
        {
            // given
            var mapping = new Mock<ICollectionMappingProvider>();
            mapping.SetupGet(instance => instance.PropertyInfo).Returns(new TestPropertyInfo(collectionType));
            mapping.SetupGet(instance => instance.StoreAs).Returns(StoreAs.Undefined);

            // when
            var shouldApply = _rdfListConvention.ShouldApply(mapping.Object);

            // then
            shouldApply.Should().BeFalse();
        }

        [Test]
        public void Should_not_be_applied_for_non_collections()
        {
            // given
            var mapping = new Mock<ICollectionMappingProvider>();
            mapping.SetupGet(instance => instance.PropertyInfo).Returns(new TestPropertyInfo(typeof(int)));
            mapping.SetupGet(instance => instance.StoreAs).Returns(StoreAs.Undefined);

            // when
            var shouldApply = _rdfListConvention.ShouldApply(mapping.Object);

            // then
            shouldApply.Should().BeFalse();
        }

        [Test]
        public void Applying_should_set_StorageStrategy()
        {
            // given
            var mapping = new Mock<ICollectionMappingProvider>();
            mapping.SetupGet(instance => instance.PropertyInfo).Returns(new TestPropertyInfo(typeof(int)));
            mapping.SetupGet(instance => instance.StoreAs).Returns(StoreAs.Undefined);
            mapping.SetupSet(instance => instance.StoreAs = StoreAs.RdfList);

            // when
            _rdfListConvention.Apply(mapping.Object);

            // then
            mapping.VerifySet(instance => instance.StoreAs = StoreAs.RdfList, Times.Once);
        }
    }
}