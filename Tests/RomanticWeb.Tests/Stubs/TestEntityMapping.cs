using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using RomanticWeb.Converters;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Mapping.Visitors;

namespace RomanticWeb.Tests.Stubs
{
    public class TestEntityMapping<TEntity> : IEntityMapping
    {
        private readonly IList<IClassMapping> _classes = new List<IClassMapping>();
        private readonly IList<IPropertyMapping> _properties = new List<IPropertyMapping>();

        public Type EntityType
        {
            get
            {
                return typeof(TEntity);
            }
        }

        public IEnumerable<IClassMapping> Classes
        {
            get
            {
                return _classes;
            }
        }

        public IEnumerable<IPropertyMapping> Properties
        {
            get
            {
                return _properties;
            }
        }

        public IEnumerable<IPropertyMapping> HiddenProperties
        {
            get
            {
                return new IPropertyMapping[0];
            }
        }

        public IPropertyMapping PropertyFor(string propertyName)
        {
            return _properties.FirstOrDefault(p => p.Name == propertyName);
        }

        public void Accept(IMappingModelVisitor mappingModelVisitor)
        {
            mappingModelVisitor.Visit(this);

            foreach (var propertyMapping in Properties)
            {
                mappingModelVisitor.Visit(propertyMapping);
            }

            foreach (var classMapping in Classes)
            {
                mappingModelVisitor.Visit(classMapping);
            }
        }

        protected void Class(Uri clazz)
        {
            var mapping = new Mock<IQueryableClassMapping>();
            mapping.SetupGet(instance => instance.Uris).Returns(new[] { clazz });
            mapping.SetupGet(instance => instance.Uri).Returns(clazz);
            _classes.Add(mapping.Object);
        }

        protected void Property(string name, Uri predicate, Type returnType, INodeConverter converter)
        {
            var property = new Mock<IPropertyMapping>();
            property.SetupGet(instance => instance.Name).Returns(name);
            property.SetupGet(instance => instance.Uri).Returns(predicate);
            property.SetupGet(instance => instance.ReturnType).Returns(returnType);
            property.SetupGet(instance => instance.Converter).Returns(converter);
            property.SetupGet(instance => instance.EntityMapping).Returns(this);
            _properties.Add(property.Object);
        }

        protected void Collection(string name, Uri predicate, Type returnType, INodeConverter converter)
        {
            var collection = new Mock<ICollectionMapping>();
            collection.SetupGet(instance => instance.Name).Returns(name);
            collection.SetupGet(instance => instance.Uri).Returns(predicate);
            collection.SetupGet(instance => instance.ReturnType).Returns(returnType);
            collection.SetupGet(instance => instance.Converter).Returns(converter);
            collection.SetupGet(instance => instance.ElementConverter).Returns(converter);
            collection.SetupGet(instance => instance.StoreAs).Returns(StoreAs.SimpleCollection);
            collection.SetupGet(instance => instance.EntityMapping).Returns(this);
            _properties.Add(collection.Object);
        }

        protected void RdfList(string name, Uri predicate, Type returnType)
        {
            var collection = new Mock<ICollectionMapping>();
            collection.SetupGet(instance => instance.Name).Returns(name);
            collection.SetupGet(instance => instance.Uri).Returns(predicate);
            collection.SetupGet(instance => instance.ReturnType).Returns(returnType);
            collection.SetupGet(instance => instance.Converter).Returns(new AsEntityConverter<IEntity>());
            collection.SetupGet(instance => instance.ElementConverter).Returns(new AsEntityConverter<IEntity>());
            collection.SetupGet(instance => instance.StoreAs).Returns(StoreAs.RdfList);
            collection.SetupGet(instance => instance.EntityMapping).Returns(this);
            _properties.Add(collection.Object);
        }
    }
}