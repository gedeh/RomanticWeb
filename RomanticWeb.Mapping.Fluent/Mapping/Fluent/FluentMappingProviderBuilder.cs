using System;
using System.Collections.Generic;
using System.Linq;
using RomanticWeb.Diagnostics;
using RomanticWeb.Mapping.Providers;
using RomanticWeb.Mapping.Visitors;

namespace RomanticWeb.Mapping.Fluent
{
    internal class FluentMappingProviderBuilder : IFluentMapsVisitor
    {
        private readonly ILogger _log;
        private Type _currentType;

        public FluentMappingProviderBuilder(ILogger log)
        {
            _log = log;
        }

        public IEntityMappingProvider Visit(EntityMap entityMap)
        {
            _currentType = entityMap.Type;
            return new EntityMappingProvider(entityMap.Type, GetClasses(entityMap), GetProperties(entityMap));
        }

        public IClassMappingProvider Visit(ClassMap classMap)
        {
            if (classMap.TermUri != null)
            {
                return new ClassMappingProvider(_currentType, classMap.TermUri, _log);
            }

            return new ClassMappingProvider(_currentType, classMap.NamespacePrefix, classMap.TermName, _log);
        }

        public IPropertyMappingProvider Visit(PropertyMap propertyMap)
        {
            return CreatePropertyMapping(propertyMap, _log);
        }

        public IPropertyMappingProvider Visit(DictionaryMap dictionaryMap, IPredicateMappingProvider key, IPredicateMappingProvider value)
        {
            var propertyMapping = CreatePropertyMapping(dictionaryMap, _log);
            return new DictionaryMappingProvider(propertyMapping, key, value);
        }

        public IPropertyMappingProvider Visit(CollectionMap collectionMap)
        {
            var result = new CollectionMappingProvider(CreatePropertyMapping(collectionMap, _log), collectionMap.StorageStrategy);
            if (collectionMap.ElementConverterType != null)
            {
                result.ElementConverterType = collectionMap.ElementConverterType;
            }

            return result;
        }

        public IPredicateMappingProvider Visit(DictionaryMap.KeyMap keyMap)
        {
            KeyMappingProvider provider;
            if (keyMap.TermUri != null)
            {
                provider = new KeyMappingProvider(keyMap.TermUri, _log);
            }
            else if (keyMap.NamespacePrefix != null && keyMap.TermName != null)
            {
                return new KeyMappingProvider(keyMap.NamespacePrefix, keyMap.TermName, _log);
            }
            else
            {
                provider = new KeyMappingProvider(_log);
            }

            if (keyMap.ConverterType != null)
            {
                provider.ConverterType = keyMap.ConverterType;
            }

            return provider;
        }

        public IPredicateMappingProvider Visit(DictionaryMap.ValueMap valueMap)
        {
            ValueMappingProvider provider;

            if (valueMap.TermUri != null)
            {
                provider = new ValueMappingProvider(valueMap.TermUri, _log);
            }
            else if (valueMap.NamespacePrefix != null && valueMap.TermName != null)
            {
                provider = new ValueMappingProvider(valueMap.NamespacePrefix, valueMap.TermName, _log);
            }
            else
            {
                provider = new ValueMappingProvider(_log);
            }

            if (valueMap.ConverterType != null)
            {
                provider.ConverterType = valueMap.ConverterType;
            }

            return provider;
        }

        private static PropertyMappingProvider CreatePropertyMapping(PropertyMapBase propertyMap, ILogger log)
        {
            PropertyMappingProvider propertyMappingProvider;
            if (propertyMap.TermUri != null)
            {
                propertyMappingProvider = new PropertyMappingProvider(propertyMap.TermUri, propertyMap.PropertyInfo, log);
            }
            else
            {
                propertyMappingProvider = new PropertyMappingProvider(propertyMap.NamespacePrefix, propertyMap.TermName, propertyMap.PropertyInfo, log);
            }

            if (propertyMap.ConverterType != null)
            {
                propertyMappingProvider.ConverterType = propertyMap.ConverterType;
            }

            return propertyMappingProvider;
        }

        private IEnumerable<IClassMappingProvider> GetClasses(EntityMap entityMap)
        {
            return entityMap.Classes.Select(c => c.Accept(this)).ToList();
        }

        private IEnumerable<IPropertyMappingProvider> GetProperties(EntityMap entityMap)
        {
            return entityMap.Properties.Select(p => p.Accept(this)).ToList();
        }
    }
}