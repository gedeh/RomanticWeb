using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RomanticWeb.Collections;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Mapping.Providers;
using RomanticWeb.Mapping.Sources;
using RomanticWeb.Mapping.Visitors;

namespace RomanticWeb.Mapping
{
    /// <summary>Default implementation of <see cref="IMappingsRepository"/>.</summary>
    internal sealed class MappingsRepository : IMappingsRepository
    {
        private readonly IList<IMappingProviderSource> _sources;
        private readonly IDictionary<Type, IEntityMapping> _mappings;
        private readonly IDictionary<Type, IEntityMapping> _genericMappings;
        private readonly IDictionary<Type, IEntityMappingProvider> _openGenericProviders;
        private readonly IList<IMappingModelVisitor> _modelVisitors;
        private readonly IList<IMappingProviderVisitor> _providerVisitors;
        private readonly MappingModelBuilder _mappingBuilder;

        public MappingsRepository(
            MappingModelBuilder mappingBuilder,
            IEnumerable<IMappingProviderSource> sources,
            IEnumerable<IMappingProviderVisitor> providerVisitors,
            IEnumerable<IMappingModelVisitor> modelVisitors)
        {
            _modelVisitors = modelVisitors.ToList();
            _providerVisitors = providerVisitors.ToList();
            _sources = sources.ToList();
            _mappings = new Dictionary<Type, IEntityMapping>();
            _genericMappings = new Dictionary<Type, IEntityMapping>();
            _openGenericProviders = new Dictionary<Type, IEntityMappingProvider>();
            _mappingBuilder = mappingBuilder;

            CreateMappings(Sources.ToArray());
            CreateMappings(_providerVisitors.OfType<IMappingProviderSource>().ToArray());
        }

        internal IEnumerable<IMappingProviderSource> Sources { get { return _sources; } }

        /// <summary>Gets a mapping for an Entity type.</summary>
        /// <typeparam name="TEntity">Entity type, for which mappings is going to be retrieved.</typeparam>
        public IEntityMapping MappingFor<TEntity>()
        {
            return MappingFor(typeof(TEntity));
        }

        /// <inheritdoc />
        public IEntityMapping MappingFor(Type entityType)
        {
            IEntityMapping result;
            if ((entityType.GetTypeInfo().IsGenericType) && (!entityType.GetTypeInfo().IsGenericTypeDefinition))
            {
                lock (_genericMappings)
                {
                    if (_genericMappings.TryGetValue(entityType, out result))
                    {
                        return result;
                    }
                }

                var genericDefinition = entityType.GetGenericTypeDefinition();
                if (_openGenericProviders.ContainsKey(genericDefinition))
                {
                    result = CreateMappingFromGenericDefinition(genericDefinition, entityType);
                    lock (_genericMappings)
                    {
                        _genericMappings[entityType] = result;
                    }

                    return result;
                }
            }

            return (_mappings.TryGetValue(entityType, out result) ? result : null);
        }

        /// <inheritdoc />
        public IPropertyMapping MappingForProperty(Uri predicateUri)
        {
            return (from mapping in _mappings
                    let type = mapping.Value
                    from property in type.Properties
                    where property.Uri.AbsoluteUri == predicateUri.AbsoluteUri
                    select property).FirstOrDefault();
        }

        /// <inheritdoc />
        public IEnumerator<IEntityMapping> GetEnumerator()
        {
            return _mappings.Values.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void CreateMappings(IEnumerable<IMappingProviderSource> sources)
        {
            lock (_mappings)
            {
                var mappings = sources
                    .SelectMany(source => source.GetMappingProviders())
                    .GroupBy(provider => provider.EntityType)
                    .Select(g => new KeyValuePair<Type, IList<IEntityMappingProvider>>(g.Key, g.ToList()))
                    .ToList()
                    .TopologicSort()
                    .Reverse();

                var singleProviderPerType = mappings.Select(provider => provider.Value.Count > 1 ? new MultiMappingProvider(provider.Key, provider.Value) : provider.Value[0]).ToList();

                var inheritanceMappingBuilder = new InheritanceMappingBuilder(singleProviderPerType);
                foreach (var provider in inheritanceMappingBuilder.CombineInheritingMappings().Where(p => p.Properties.Any() || p.Classes.Any()))
                {
                    foreach (var visitor in _providerVisitors)
                    {
                        provider.Accept(visitor);
                    }

                    if (provider.EntityType.GetTypeInfo().IsGenericTypeDefinition)
                    {
                        _openGenericProviders[provider.EntityType] = provider;
                    }

                    StoreMapping(_mappingBuilder.BuildMapping(provider));
                }
            }
        }

        private IEntityMapping CreateMappingFromGenericDefinition(Type genericDefinition, Type entityType)
        {
            var openGenericProvider = _openGenericProviders[genericDefinition];
            var provider = new ClosedGenericEntityMappingProvider(openGenericProvider, entityType.GenericTypeArguments);

            foreach (var visitor in _providerVisitors)
            {
                provider.Accept(visitor);
            }

            return _mappingBuilder.BuildMapping(provider);
        }

        private void StoreMapping(IEntityMapping mapping)
        {
            if (_mappings.ContainsKey(mapping.EntityType))
            {
                throw new MappingException(string.Format("Duplicate mapping for type {0}", mapping.EntityType));
            }

            _mappings.Add(mapping.EntityType, mapping);

            foreach (var mappingModelVisitor in _modelVisitors)
            {
                mapping.Accept(mappingModelVisitor);
            }
        }
    }
}