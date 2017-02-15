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
            mapping.SetupGet(instance => instance.PropertyInfo).Returns(typeof(TestPropertyInfo).GetProperty("IntegerList"));
            mapping.SetupGet(instance => instance.StoreAs).Returns(StoreAs.Undefined);

            // when
            var shouldApply = _rdfListConvention.ShouldApply(mapping.Object);

            // then
            shouldApply.Should().BeTrue();
        }

        [TestCase("IntegerCollection")]
        [TestCase("IntegerEnumeration")]
        [TestCase("IntegerSet")]
        [TestCase("IntegerStringDictionary")]
        public void Should_not_be_applied_for_non_list_collections(string propertyName)
        {
            // given
            var mapping = new Mock<ICollectionMappingProvider>();
            mapping.SetupGet(instance => instance.PropertyInfo).Returns(typeof(TestPropertyInfo).GetProperty(propertyName));
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
            mapping.SetupGet(instance => instance.PropertyInfo).Returns(typeof(TestPropertyInfo).GetProperty("Integer"));
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
            mapping.SetupGet(instance => instance.PropertyInfo).Returns(typeof(TestPropertyInfo).GetProperty("Integer"));
            mapping.SetupGet(instance => instance.StoreAs).Returns(StoreAs.Undefined);
            mapping.SetupSet(instance => instance.StoreAs = StoreAs.RdfList);

            // when
            _rdfListConvention.Apply(mapping.Object);

            // then
            mapping.VerifySet(instance => instance.StoreAs = StoreAs.RdfList, Times.Once);
        }
    }
}