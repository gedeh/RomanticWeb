using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using RomanticWeb.Entities.Proxies;
using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Model;
using RomanticWeb.NamedGraphs;

namespace RomanticWeb.Entities
{
    internal class InternalProxyCaster : IEntityCaster
    {
        private static readonly EntityMapping EntityMapping = new EntityMapping(typeof(IEntity));

        private readonly Func<Entity, IEntityMapping, IEntityProxy> _createProxy;
        private readonly IDictionary<IEntityContext, IDictionary<Entity, IDictionary<int, dynamic>>> _cache;
        private readonly IMappingsRepository _mappings;
        private readonly INamedGraphSelector _graphSelector;
        private readonly IEntityStore _store;
        private readonly IEntityMapping _typedEntityMapping;
        private readonly IPropertyMapping _typesPropertyMapping;

        public InternalProxyCaster(
            Func<Entity, IEntityMapping, IEntityProxy> proxyFactory,
            IMappingsRepository mappings,
            INamedGraphSelector graphSelector,
            IEntityStore store)
        {
            _createProxy = proxyFactory;
            _mappings = mappings;
            _graphSelector = graphSelector;
            _store = store;
            _typedEntityMapping = _mappings.MappingFor<ITypedEntity>();
            _typesPropertyMapping = _typedEntityMapping.PropertyFor("Types");
            _cache = new ConcurrentDictionary<IEntityContext, IDictionary<Entity, IDictionary<int, dynamic>>>();
        }

        public T EntityAs<T>(Entity entity, Type[] types) where T : IEntity
        {
            return (T)EntityAs(entity, typeof(T), types);
        }

        private dynamic EntityAs(Entity entity, Type requested, Type[] types)
        {
            int key = types.Except(new[] { requested }).Aggregate(requested.GetHashCode(), (current, additionalType) => current ^ additionalType.GetHashCode());
            dynamic result = GetFromCache(entity, key);
            if (result != null)
            {
                return result;
            }

            IEntityMapping mapping;
            switch (types.Length)
            {
                case 1:
                    mapping = GetMapping(types[0]);
                    break;
                case 0:
                    types = new[] { requested };
                    mapping = GetMapping(requested);
                    break;
                default:
                    mapping = new MultiMapping(types.Select(GetMapping).ToArray());
                    break;
            }

            AssertEntityTypes(entity, mapping);
            return EntityAs(entity, mapping, types, key);
        }

        private dynamic EntityAs(Entity entity, IEntityMapping mapping, Type[] types, int key)
        {
            var proxy = _createProxy(entity, mapping);
            return GetFromCache(entity, key, proxy.ActLike(types));
        }

        private void AssertEntityTypes(Entity entity, IEntityMapping entityMapping)
        {
            Uri graphName = (_graphSelector != null ? _graphSelector.SelectGraph(entity.Id, _typedEntityMapping, _typesPropertyMapping) : null);
            var currentTypes = _store.GetObjectsForPredicate(entity.Id, Vocabularies.Rdf.type, graphName);
            var additionalTypes = (from @class in entityMapping.Classes
                                   let additionalType = Model.Node.ForUri(@class.Uri)
                                   where !currentTypes.Contains(additionalType)
                                   select additionalType).ToList();
            if (additionalTypes.Count > 0)
            {
                _store.ReplacePredicateValues(
                    entity.Id,
                    Model.Node.ForUri(Vocabularies.Rdf.type),
                    () => currentTypes.Union(additionalTypes),
                    graphName,
                    entity.Context.CurrentCulture);
            }
        }

        private IEntityMapping GetMapping(Type type)
        {
            if (type == typeof(IEntity))
            {
                return EntityMapping;
            }

            var mapping = _mappings.MappingFor(type);
            if (mapping == null)
            {
                throw new UnMappedTypeException(type);
            }

            return mapping;
        }

        private dynamic GetFromCache(Entity entity, int key, dynamic itemToSet = null)
        {
            IDictionary<Entity, IDictionary<int, dynamic>> entityCache;
            if (!_cache.TryGetValue(entity.Context, out entityCache))
            {
                _cache[entity.Context] = entityCache = new ConcurrentDictionary<Entity, IDictionary<int, dynamic>>();
                entity.Context.Disposed += () => _cache.Remove(entity.Context);
            }

            IDictionary<int, dynamic> typeCache;
            if (!entityCache.TryGetValue(entity, out typeCache))
            {
                entityCache[entity] = typeCache = new ConcurrentDictionary<int, dynamic>();
            }

            if (itemToSet != null)
            {
                return typeCache[key] = itemToSet;
            }

            dynamic result;
            return (typeCache.TryGetValue(key, out result) ? result : null);
        }
    }
}