using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RomanticWeb.Converters;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Conventions;
using RomanticWeb.Mapping.Providers;
using RomanticWeb.TestEntities;
using RomanticWeb.Tests.Stubs;

namespace RomanticWeb.Tests.Mapping.Conventions
{
    [TestFixture]
    public class EntityPropertiesConventionTests
    {
        private EntityPropertiesConvention _convention;

        [SetUp]
        public void Setup()
        {
            _convention = new EntityPropertiesConvention();
        }

        [TestCase("TestEntity")]
        [TestCase("TestEntityList", Description = "Should work for collection type")]
        [TestCase("Entity")]
        [TestCase("EntityList", Description = "Should work for collection type")]
        public void Should_be_applied_to_property_IEntity_property_type(string propertyName)
        {
            // given
            var convention = new Mock<IPropertyMappingProvider>();
            convention.SetupGet(instance => instance.PropertyInfo).Returns(typeof(TestPropertyInfo).GetProperty(propertyName));
            convention.SetupGet(instance => instance.ConverterType).Returns(default(Type));

            // when
            var shouldApply = _convention.ShouldApply(convention.Object);

            // then
            shouldApply.Should().BeTrue();
        }

        [TestCase("TestEntity", typeof(AsEntityConverter<ITestEntity>))]
        [TestCase("TestEntityList", typeof(AsEntityConverter<ITestEntity>), Description = "Should work for collection type")]
        [TestCase("Person", typeof(AsEntityConverter<IPerson>))]
        [TestCase("EntityList", typeof(AsEntityConverter<IEntity>), Description = "Should work for collection type")]
        public void Should_set_converter_for_property_with_IEntity_property_type(string propertyName, Type converterType)
        {
            // given
            var convention = new Mock<IPropertyMappingProvider>();
            convention.SetupGet(instance => instance.PropertyInfo).Returns(typeof(TestPropertyInfo).GetProperty(propertyName));
            convention.SetupGet(instance => instance.ConverterType).Returns(default(Type));
            convention.SetupSet(instance => instance.ConverterType = converterType);

            // when
            _convention.Apply(convention.Object);

            // then
            convention.VerifySet(instance => instance.ConverterType = converterType, Times.Once);
        }
    }
}