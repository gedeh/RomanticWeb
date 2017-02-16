using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using RomanticWeb.Mapping.Model;

namespace RomanticWeb.Dynamic
{
    /// <summary>
    /// Default implementation of <see cref="IDictionaryTypeProvider"/>, 
    /// which assumes dictionary types are named according to a pattern 
    /// based on original entity type name and property details.
    /// </summary>
    public class DefaultDictionaryTypeProvider : IDictionaryTypeProvider
    {
        private readonly IDictionary<Assembly, Assembly> _assemblyCache = new ConcurrentDictionary<Assembly, Assembly>();
        private readonly EmitHelper _emitHelper;

        /// <summary>Initializes a new instance of the <see cref="DefaultDictionaryTypeProvider"/> class.</summary>
        /// <param name="emitHelper">The code emitter helper.</param>
        public DefaultDictionaryTypeProvider(EmitHelper emitHelper)
        {
            _emitHelper = emitHelper;
        }

        /// <inheritdoc/>
        public Type GetEntryType(IPropertyMapping property)
        {
            return GetTypeDyctionaryEntityNames(property);
        }

        /// <inheritdoc/>
        public Type GetOwnerType(IPropertyMapping property)
        {
            return GetTypeDyctionaryEntityNames(property, true);
        }

        private Type GetTypeDyctionaryEntityNames(IPropertyMapping property, bool isOwnerType = false)
        {
            var entityType = property.EntityMapping.EntityType.GetTypeInfo();
            DictionaryEntityNames entityName;
            Assembly mappingAssembly;
            if (_assemblyCache.TryGetValue(entityType.Assembly, out mappingAssembly))
            {
                entityName = new DictionaryEntityNames(
                    entityType.Namespace,
                    entityType.Name,
                    property.Name,
                    mappingAssembly.FullName);
            }
            else
            {
                entityName = new TypeDictionaryEntityNames(property.EntityMapping.EntityType.GetTypeInfo().GetProperty(property.Name));
                if (Type.GetType(entityName.EntryTypeFullyQualifiedName, false) != null)
                {
                    _assemblyCache[entityType.Assembly] = mappingAssembly = entityType.Assembly;
                }
                else
                {
                    _assemblyCache[entityType.Assembly] = mappingAssembly = _emitHelper.GetDynamicModule().Assembly;
                    entityName = new DictionaryEntityNames(
                        entityType.Namespace,
                        entityType.Name,
                        property.Name,
                        mappingAssembly.FullName);
                }
            }

            return mappingAssembly.GetType(isOwnerType ? entityName.FullOwnerTypeName : entityName.FullEntryTypeName);
        }
    }
}