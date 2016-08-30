using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Mapping.Visitors;
using RomanticWeb.TestEntities.Foaf;

namespace RomanticWeb.Tests
{
    [TestFixture]
    public class RdfTypeCacheBuilderTests
    {
        private RdfTypeCacheBuilder _builder;
        private Mock<IRdfTypeCache> _cache;

        [SetUp]
        public void Setup()
        {
            _cache = new Mock<IRdfTypeCache>();
            _builder = new RdfTypeCacheBuilder(_cache.Object);
        }

        [Test]
        public void Accepting_builder_should_populate_cache()
        {
            // given
            var mapping = CreateMapping<IAgent>(Vocabularies.Foaf.Agent);

            // when
            mapping.Accept(_builder);

            // then
            _cache.Verify(c => c.Add(typeof(IAgent), It.Is<IList<IClassMapping>>(m => m.Count == 1)));
        }

        private static IEntityMapping CreateMapping<T>(params Uri[] classUris)
        {
            var classMappings = classUris.Select(CreateClassMapping);
            var mapping = new Mock<IEntityMapping>();
            mapping.SetupGet(instance => instance.EntityType).Returns(typeof(T));
            mapping.SetupGet(instance => instance.Classes).Returns(classMappings);
            mapping.Setup(instance => instance.Accept(It.IsAny<IMappingModelVisitor>())).Callback<IMappingModelVisitor>(visitor => Accept(mapping.Object, visitor));
            return mapping.Object;
        }

        private static IClassMapping CreateClassMapping(Uri uri)
        {
            var mapping = new Mock<IClassMapping>();
            mapping.SetupGet(instance => instance.IsInherited).Returns(false);
            mapping.Setup(instance => instance.IsMatch(It.IsAny<IEnumerable<Uri>>())).Callback<IEnumerable<Uri>>(uris => uris.Contains(uri));
            return mapping.Object;
        }

        private static void Accept(IEntityMapping mapping, IMappingModelVisitor visitor)
        {
            visitor.Visit(mapping);
            foreach (var classMapping in mapping.Classes)
            {
                visitor.Visit(classMapping);
            }
        }
    }
}