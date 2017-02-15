using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RomanticWeb.Converters;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Conventions;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Mapping.Providers;
using RomanticWeb.Tests.Stubs;

namespace RomanticWeb.Tests.Mapping.Conventions
{
    [TestFixture]
    public class DefaultConverterConventionTests
    {
        private DefaultConvertersConvention _convention;

        [SetUp]
        public void Setup()
        {
            _convention = new DefaultConvertersConvention();
        }

        [TearDown]
        public void Teardown()
        {
        }

        [TestCase("Integer")]
        [TestCase("IntegerList", Description = "Should work for collection type")]
        [TestCase("NullableInteger", Description = "Should work for nullable type")]
        public void Should_be_applied_to_property_without_converter_with_known_property_type(string propertyName)
        {
            // given
            _convention.SetDefault<int, IntegerConverter>();
            var mapping = MakePropertyMappingProvider(propertyName, default(Type));

            // when
            var shouldApply = ((IPropertyConvention)_convention).ShouldApply(mapping.Object);

            // then
            shouldApply.Should().BeTrue();
        }

        [Test]
        public void Should_not_be_applied_to_property_with_unknown_property_type()
        {
            // given
            var mapping = MakePropertyMappingProvider("Float", default(Type));

            // when
            var shouldApply = ((IPropertyConvention)_convention).ShouldApply(mapping.Object);

            // then
            shouldApply.Should().BeFalse();
        }

        [Test]
        public void Applying_should_set_default_converter_type_for_known_property_type()
        {
            // given
            _convention.SetDefault<int, IntegerConverter>();
            var mapping = MakePropertyMappingProvider("Integer", default(Type));

            // when
            ((IPropertyConvention)_convention).Apply(mapping.Object);

            // then
            mapping.VerifySet(instance => instance.ConverterType = typeof(IntegerConverter), Times.Once);
        }

        [Test]
        public void Applying_should_set_default_converter_for_simple_collection()
        {
            // given
            _convention.SetDefault<int, IntegerConverter>();
            var mapping = MakeCollectionMappingProvider("IntegerList", default(Type));

            // when
            ((IPropertyConvention)_convention).Apply(mapping.Object);

            // then
            mapping.VerifySet(instance => instance.ConverterType = typeof(IntegerConverter), Times.Once);
        }

        [Test]
        public void Applying_should_not_set_default_converter_for_rdf_list()
        {
            // given
            _convention.SetDefault<int, IntegerConverter>();
            var mapping = MakeCollectionMappingProvider("IntegerList", default(Type), StoreAs.RdfList);

            // when
            ((IPropertyConvention)_convention).Apply(mapping.Object);

            // then
            mapping.VerifySet(instance => instance.ConverterType = typeof(AsEntityConverter<IEntity>), Times.Once);
        }

        [Test]
        public void Applying_should_set_default_converter_for_collection_elements()
        {
            // given
            _convention.SetDefault<int, IntegerConverter>();
            var mapping = MakeCollectionMappingProvider("IntegerList", default(Type), default(StoreAs), typeof(Type));

            // when
            ((ICollectionConvention)_convention).Apply(mapping.Object);

            // then
            mapping.VerifySet(instance => instance.ElementConverterType = typeof(IntegerConverter), Times.Once);
        }

        [Test]
        public void Applying_should_set_converter_for_explicitly_defined_array_type()
        {
            // given
            _convention.SetDefault<int[], Base64BinaryConverter>();
            _convention.SetDefault<int, IntegerConverter>();
            var mapping = MakeCollectionMappingProvider("IntegerArray", default(Type));

            // when
            ((IPropertyConvention)_convention).Apply(mapping.Object);

            // then
            mapping.VerifySet(instance => instance.ConverterType = typeof(Base64BinaryConverter), Times.Once);
        }

        [Test]
        public void Applying_should_converter_to_dictionary_key()
        {
            // given
            _convention.SetDefault<int, IntegerConverter>();
            var keyMapping = MakePropertyMappingProvider(null, default(Type));
            var mapping = MakeDictionaryMappingProvider("IntegerIntegerDictionary", keyMapping.Object, MakePropertyMappingProvider(null, default(Type)).Object);

            // when
            ((IDictionaryConvention)_convention).Apply(mapping.Object);

            // then
            keyMapping.VerifySet(instance => instance.ConverterType = typeof(IntegerConverter), Times.Once);
        }

        [Test]
        public void Applying_should_converter_to_dictionary_element()
        {
            // given
            _convention.SetDefault<int, IntegerConverter>();
            var valueMapping = MakePropertyMappingProvider(null, default(Type));
            var mapping = MakeDictionaryMappingProvider("IntegerIntegerDictionary", MakePropertyMappingProvider(null, default(Type)).Object, valueMapping.Object);

            // when
            ((IDictionaryConvention)_convention).Apply(mapping.Object);

            // then
            valueMapping.VerifySet(instance => instance.ConverterType = typeof(IntegerConverter), Times.Once);
        }

        private Mock<IPropertyMappingProvider> MakePropertyMappingProvider(string propertyName, Type converterType)
        {
            var result = new Mock<IPropertyMappingProvider>();
            result.SetupGet(instance => instance.PropertyInfo).Returns(propertyName != null ? typeof(TestPropertyInfo).GetProperty(propertyName) : null);
            result.SetupGet(instance => instance.ConverterType).Returns(() => converterType);
            result.SetupSet(instance => instance.ConverterType = It.IsAny<Type>()).Callback<Type>(type => converterType = type);
            return result;
        }

        private Mock<ICollectionMappingProvider> MakeCollectionMappingProvider(string propertyName, Type converterType, StoreAs storeAs = default(StoreAs), Type elementConverterType = null)
        {
            var result = new Mock<ICollectionMappingProvider>();
            result.SetupGet(instance => instance.PropertyInfo).Returns(typeof(TestPropertyInfo).GetProperty(propertyName));
            result.SetupGet(instance => instance.ConverterType).Returns(converterType);
            result.SetupSet(instance => instance.ConverterType = It.IsAny<Type>());
            result.SetupGet(instance => instance.StoreAs).Returns(storeAs);
            result.SetupSet(instance => instance.StoreAs = It.IsAny<StoreAs>());
            result.SetupGet(instance => instance.ElementConverterType).Returns(elementConverterType);
            result.SetupSet(instance => instance.ElementConverterType = It.IsAny<Type>());
            return result;
        }

        private Mock<IDictionaryMappingProvider> MakeDictionaryMappingProvider(string propertyName, IPredicateMappingProvider key, IPredicateMappingProvider value)
        {
            var result = new Mock<IDictionaryMappingProvider>();
            result.SetupGet(instance => instance.PropertyInfo).Returns(typeof(TestPropertyInfo).GetProperty(propertyName));
            result.SetupGet(instance => instance.Key).Returns(key);
            result.SetupGet(instance => instance.Value).Returns(value);
            return result;
        }
    }
}