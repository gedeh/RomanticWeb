using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RomanticWeb.Mapping.Providers;
using RomanticWeb.Mapping.Sources;
using RomanticWeb.Mapping.Visitors;
using RomanticWeb.TestEntities.MixedMappings;

namespace RomanticWeb.Tests.Mapping
{
    [TestFixture]
    public class ClosedGenericEntityMappingProviderTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Should_be_for_closed_entity_type()
        {
            // given
            var provider = new ClosedGenericEntityMappingProvider(CreateOpenGenericProvider("MappedProperty1"), typeof(int));

            // when
            var entityType = provider.EntityType;

            // then
            entityType.Should().Be<IGenericParent<int>>();
        }

        [TestCase("MappedProperty1", typeof(int))]
        [TestCase("GenericProperty", typeof(ICollection<int>))]
        public void Should_return_mappings_for_closed_properties(string propertyName, Type expectedType)
        {
            // given
            var provider = new ClosedGenericEntityMappingProvider(CreateOpenGenericProvider(propertyName), typeof(int));

            // when
            var propertyMapping = provider.Properties.Single();

            // then
            propertyMapping.Should().NotBeNull();
            propertyMapping.PropertyInfo.PropertyType.Should().Be(expectedType);
        }

        private IEntityMappingProvider CreateOpenGenericProvider(string propertyName)
        {
            var type = typeof(IGenericParent<>);
            var _provider = new Mock<VisitableEntityMappingProviderBase>();
            _provider.SetupGet(p => p.EntityType).Returns(type);
            _provider.SetupGet(p => p.Properties).Returns(new[] { CreatePropertyMappingProvider(type.GetProperty(propertyName)) });
            _provider.SetupGet(p => p.Classes).Returns(new List<IClassMappingProvider>());

            return _provider.Object;
        }

        private IPropertyMappingProvider CreatePropertyMappingProvider(PropertyInfo property)
        {
            var result = new Mock<IPropertyMappingProvider>();
            result.SetupGet(instance => instance.PropertyInfo).Returns(property);
            result.Setup(instance => instance.Accept(It.IsAny<IMappingProviderVisitor>()))
                .Callback<IMappingProviderVisitor>(visitor => Accept(result.Object, visitor));
            return result.Object;
        }

        private void Accept(IPropertyMappingProvider provider, IMappingProviderVisitor mappingProviderVisitor)
        {
            mappingProviderVisitor.Visit(provider);
        }
    }
}