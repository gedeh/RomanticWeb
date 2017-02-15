using System;
using System.Collections.Generic;
using System.Reflection;
using Moq;
using RomanticWeb.Mapping.Providers;
using RomanticWeb.Mapping.Visitors;
using RomanticWeb.Ontologies;

namespace RomanticWeb.Tests.Mapping
{
    internal class TestingInheritanceTreeProvider : InheritanceTreeProvider
    {
        public TestingInheritanceTreeProvider()
            : base(new ChildMap(), CreateParentProviders())
        {
        }

        private interface IChildEntity : IParentLevel1
        {
        }

        private interface IParentLevel1 : IParentLevel2
        {
            string InParent1 { get; set; }
        }

        private interface IParentLevel2 : IParentLevel3
        {
            string InParent2 { get; set; }
        }

        private interface IParentLevel3
        {
            string InParent3 { get; set; }
        }

        private static IEnumerable<IEntityMappingProvider> CreateParentProviders()
        {
            yield return new Parent1Map();
            yield return new Parent2Map();
            yield return new Parent3Map();
        }

        private class ChildMap : IEntityMappingProvider
        {
            public Type EntityType
            {
                get
                {
                    return typeof(IChildEntity);
                }
            }

            public IEnumerable<IClassMappingProvider> Classes
            {
                get
                {
                    yield break;
                }
            }

            public IEnumerable<IPropertyMappingProvider> Properties
            {
                get
                {
                    var result = new Mock<IPropertyMappingProvider>();
                    result.SetupGet(instance => instance.PropertyInfo).Returns(typeof(IParentLevel3).GetTypeInfo().GetProperty("InParent3"));
                    result.Setup(instance => instance.GetTerm).Returns(GetUri);
                    yield return result.Object;
                }
            }

            public void Accept(IMappingProviderVisitor mappingProviderVisitor)
            {
            }

            private Uri GetUri(IOntologyProvider p)
            {
                return new Uri("urn:override:parent3");
            }
        }

        private class Parent1Map : IEntityMappingProvider
        {
            public Type EntityType
            {
                get
                {
                    return typeof(IParentLevel1);
                }
            }

            public IEnumerable<IClassMappingProvider> Classes
            {
                get
                {
                    yield break;
                }
            }

            public IEnumerable<IPropertyMappingProvider> Properties
            {
                get
                {
                    var result = new Mock<IPropertyMappingProvider>();
                    result.SetupGet(instance => instance.PropertyInfo).Returns(EntityType.GetProperty("InParent1"));
                    result.SetupGet(instance => instance.GetTerm).Returns(GetUri);
                    yield return result.Object;
                }
            }

            public void Accept(IMappingProviderVisitor mappingProviderVisitor)
            {
            }

            private Uri GetUri(IOntologyProvider p)
            {
                return new Uri("urn:parent:level1");
            }
        }

        private class Parent2Map : IEntityMappingProvider
        {
            public Type EntityType
            {
                get
                {
                    return typeof(IParentLevel2);
                }
            }

            public IEnumerable<IClassMappingProvider> Classes
            {
                get
                {
                    yield break;
                }
            }

            public IEnumerable<IPropertyMappingProvider> Properties
            {
                get
                {
                    var result = new Mock<IPropertyMappingProvider>();
                    result.SetupGet(instance => instance.PropertyInfo).Returns(EntityType.GetProperty("InParent2"));
                    result.Setup(instance => instance.GetTerm).Returns(GetUri);
                    yield return result.Object;
                }
            }

            public void Accept(IMappingProviderVisitor mappingProviderVisitor)
            {
            }

            private Uri GetUri(IOntologyProvider p)
            {
                return new Uri("urn:parent:level2");
            }
        }

        private class Parent3Map : IEntityMappingProvider
        {
            public Type EntityType
            {
                get
                {
                    return typeof(IParentLevel3);
                }
            }

            public IEnumerable<IClassMappingProvider> Classes
            {
                get
                {
                    yield break;
                }
            }

            public IEnumerable<IPropertyMappingProvider> Properties
            {
                get
                {
                    var result = new Mock<IPropertyMappingProvider>();
                    result.SetupGet(instance => instance.PropertyInfo).Returns(EntityType.GetProperty("InParent3"));
                    result.Setup(instance => instance.GetTerm).Returns(GetUri);
                    yield return result.Object;
                }
            }

            public void Accept(IMappingProviderVisitor mappingProviderVisitor)
            {
            }

            private Uri GetUri(IOntologyProvider p)
            {
                return new Uri("urn:parent:level3");
            }
        }
    }
}