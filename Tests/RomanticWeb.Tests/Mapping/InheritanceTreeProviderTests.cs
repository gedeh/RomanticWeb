using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RomanticWeb.Mapping.Providers;
using RomanticWeb.Ontologies;
using RomanticWeb.TestEntities.MixedMappings;
using RomanticWeb.Tests.Stubs;
using VDS.RDF;

namespace RomanticWeb.Tests.Mapping
{
    [TestFixture]
    public class InheritanceTreeProviderTests
    {
        private IOntologyProvider _ontology;

        [SetUp]
        public void Setup()
        {
            _ontology = new Mock<IOntologyProvider>().Object;
        }

        [Test]
        public void Should_ignore_overriden_parent_properties()
        {
            // given
            var child = CreateEntity(typeof(IDerivedLevel2), typeof(IDerivedLevel2), new Uri("urn:in:child"));
            var parent = CreateEntity(typeof(IDerived), typeof(IDerived), new Uri("urn:in:parent"));

            // when
            var provider = new InheritanceTreeProvider(child, parent.AsEnumerable());

            // then
            provider.Properties.Should().HaveCount(1);
            provider.Properties.Single().GetTerm(_ontology).Should().Be(new Uri("urn:in:child"));
        }

        private IEntityMappingProvider CreateEntity(Type entityType, Type propertyType, Uri uri)
        {
            var propertyMapping = new Mock<IPropertyMappingProvider>();
            propertyMapping.SetupGet(instance => instance.PropertyInfo).Returns(new TestPropertyInfo(GetType(), propertyType));
            propertyMapping.Setup(instance => instance.GetTerm).Returns(provider => uri);

            var result = new Mock<IEntityMappingProvider>();
            result.SetupGet(instance => instance.EntityType).Returns(entityType);
            result.SetupGet(instance => instance.Properties).Returns(propertyMapping.Object.AsEnumerable());
            return result.Object;
        }

        private IPropertyMappingProvider CreateProperty(PropertyInfo property, Uri uri)
        {
            var result = new Mock<IPropertyMappingProvider>();
            result.SetupGet(instance => instance.PropertyInfo).Returns(property);
            result.Setup(instance => instance.GetTerm).Returns(provider => uri);
            return result.Object;
        }
    }
}