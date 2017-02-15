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
    public class CollectionConventionTests
    {
        private CollectionStorageConvention _collectionStorageConvention;

        [SetUp]
        public void Setup()
        {
            _collectionStorageConvention = new CollectionStorageConvention();
        }

        [TestCase("IntegerEnumeration")]
        [TestCase("IntegerCollection")]
        public void Should_be_applied_for_simple_collections(string propertyName)
        {
            // given
            var mapping = new Mock<ICollectionMappingProvider>();
            mapping.SetupGet(instance => instance.PropertyInfo).Returns(typeof(TestPropertyInfo).GetProperty(propertyName));
            mapping.SetupGet(instance => instance.StoreAs).Returns(default(StoreAs));

            // when
            var shouldApply = _collectionStorageConvention.ShouldApply(mapping.Object);

            // then
            shouldApply.Should().BeTrue();
        }

        [TestCase("IntegerList")]
        [TestCase("IntegerSet")]
        [TestCase("IntegerStringDictionary")]
        public void Should_not_be_applied_for_comple_collections(string propertyName)
        {
            // given
            var mapping = new Mock<ICollectionMappingProvider>();
            mapping.SetupGet(instance => instance.PropertyInfo).Returns(typeof(TestPropertyInfo).GetProperty(propertyName));
            mapping.SetupGet(instance => instance.StoreAs).Returns(default(StoreAs));

            // when
            var shouldApply = _collectionStorageConvention.ShouldApply(mapping.Object);

            // then
            shouldApply.Should().BeFalse();
        }

        [Test]
        public void Should_not_be_applied_for_non_collections()
        {
            // given
            var mapping = new Mock<ICollectionMappingProvider>();
            mapping.SetupGet(instance => instance.PropertyInfo).Returns(typeof(TestPropertyInfo).GetProperty("Integer"));
            mapping.SetupGet(instance => instance.StoreAs).Returns(default(StoreAs));

            // when
            var shouldApply = _collectionStorageConvention.ShouldApply(mapping.Object);

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
            mapping.SetupSet(instance => instance.StoreAs = StoreAs.SimpleCollection);

            // when
            _collectionStorageConvention.Apply(mapping.Object);

            // then
            mapping.VerifySet(instance => instance.StoreAs = StoreAs.SimpleCollection, Times.Once);
        }
    }
}