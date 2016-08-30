using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RomanticWeb.Collections;
using RomanticWeb.Mapping.Providers;
using RomanticWeb.Mapping.Visitors;

namespace RomanticWeb.Tests.Collections
{
    [TestFixture]
    public class DependencyTreeTests
    {
        private IMappingProviderVisitor[] _expected;
        private DependencyTree<IMappingProviderVisitor> _dependencyTree;

        [Test]
        public void Should_enumerate_model_transformers_in_a_correct_order()
        {
            int index = 0;
            foreach (var instance in _dependencyTree)
            {
                instance.Should().Be(_expected[index]);
                index++;
            }
        }

        [SetUp]
        public void Setup()
        {
            var instances = new IMappingProviderVisitor[]
                {
                    new YetAnotherDependentyResponseModelTransformer(),
                    new SomeAnotherDependentyResponseModelTransformer(),
                    new SomeDependentyResponseModelTransformer(),
                    new SomeResponseModelTransformer()
                };
            _dependencyTree = new DependencyTree<IMappingProviderVisitor>(instances);
            _expected = new[]
                {
                    instances[3],
                    instances[2],
                    instances[0],
                    instances[1]
                };
        }

        [TearDown]
        public void Teardown()
        {
            _dependencyTree = null;
        }

        private class SomeResponseModelTransformer : IMappingProviderVisitor
        {
            public IEnumerable<Type> Requires { get { return Type.EmptyTypes; } }

            public void Visit(ICollectionMappingProvider collectionMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IPropertyMappingProvider propertyMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IDictionaryMappingProvider dictionaryMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IClassMappingProvider classMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IEntityMappingProvider entityMappingProvider)
            {
                throw new NotImplementedException();
            }
        }

        private class SomeDependentyResponseModelTransformer : IMappingProviderVisitor
        {
            public IEnumerable<Type> Requires { get { return new[] { typeof(SomeResponseModelTransformer) }; } }

            public void Visit(ICollectionMappingProvider collectionMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IPropertyMappingProvider propertyMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IDictionaryMappingProvider dictionaryMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IClassMappingProvider classMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IEntityMappingProvider entityMappingProvider)
            {
                throw new NotImplementedException();
            }
        }

        private class SomeAnotherDependentyResponseModelTransformer : IMappingProviderVisitor
        {
            public IEnumerable<Type> Requires { get { return new[] { typeof(SomeDependentyResponseModelTransformer) }; } }

            public void Visit(ICollectionMappingProvider collectionMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IPropertyMappingProvider propertyMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IDictionaryMappingProvider dictionaryMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IClassMappingProvider classMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IEntityMappingProvider entityMappingProvider)
            {
                throw new NotImplementedException();
            }
        }

        private class YetAnotherDependentyResponseModelTransformer : IMappingProviderVisitor
        {
            public IEnumerable<Type> Requires { get { return new[] { typeof(SomeDependentyResponseModelTransformer) }; } }

            public void Visit(ICollectionMappingProvider collectionMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IPropertyMappingProvider propertyMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IDictionaryMappingProvider dictionaryMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IClassMappingProvider classMappingProvider)
            {
                throw new NotImplementedException();
            }

            public void Visit(IEntityMappingProvider entityMappingProvider)
            {
                throw new NotImplementedException();
            }
        }
    }
}