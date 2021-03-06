﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RomanticWeb.Entities.Proxies;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Model;

namespace RomanticWeb.Entities
{
    /// <summary>Provides useful extensions methods for entities.</summary>
    public static class EntityExtensions
    {
        /// <summary>Gets the entity as a dynamic object.</summary>
        /// <param name="entity">Target entity to be converted to dynamic.</param>
        public static dynamic AsDynamic(this IEntity entity)
        {
            dynamic result;
            entity = UnwrapProxy(entity);

            if (entity is Entity)
            {
                result = ((Entity)entity).AsDynamic();
            }
            else if (entity is EntityProxy)
            {
                result = ((EntityProxy)entity).AsDynamic();
            }
            else if (entity is IProxy)
            {
                result = ((IProxy)entity).WrappedObject;
            }
            else
            {
                result = entity;
            }

            return result;
        }

        /// <summary>Wraps the entity as a given statically typed type.</summary>
        public static TInterface AsEntity<TInterface>(this IEntity entity) where TInterface : class, IEntity
        {
            TInterface result;
            entity = UnwrapProxy(entity);

            if (entity is TInterface)
            {
                result = (TInterface)entity;
            }
            else if (entity is Entity)
            {
                result = ((Entity)entity).AsEntity<TInterface>();
            }
            else if (entity is EntityProxy)
            {
                result = ((EntityProxy)entity).AsEntity<TInterface>();
            }
            else
            {
                result = entity.ActLike<TInterface>();
            }

            return result;
        }

        /// <summary>Gets an enumeration containing all RDF types behind given entity.</summary>
        /// <param name="entity">Entity to operate on.</param>
        /// <returns>Returns an enumeration of RDF types for given entity.</returns>
        public static IEnumerable<EntityId> GetTypes(this IEntity entity)
        {
            return (entity != null ? entity.AsEntity<ITypedEntity>().Types.Union(new[] { new EntityId(Vocabularies.Owl.Thing) }) : new EntityId[0]);
        }

        /// <summary>Determines if a given entity is of the given type provided.</summary>
        /// <param name="entity">Entity to operate on.</param>
        /// <param name="type">Types to check against.</param>
        /// <returns><b>true</b> if an entity is of any of the given types; othewise <b>false</b>.</returns>
        public static bool Is(this IEntity entity, Uri type)
        {
            return entity.Is(new[] { type });
        }

        /// <summary>Determines if a given entity is of any of the types provided.</summary>
        /// <param name="entity">Entity to operate on.</param>
        /// <param name="types">Enumeration of types to check against.</param>
        /// <returns><b>true</b> if an entity is of any of the given types; othewise <b>false</b>.</returns>
        public static bool Is(this IEntity entity, IEnumerable<Uri> types)
        {
            return entity.Is(types.Select(item => new EntityId(item)));
        }

        /// <summary>Determines if a given entity is of the given type provided.</summary>
        /// <param name="entity">Entity to operate on.</param>
        /// <param name="type">Types to check against.</param>
        /// <returns><b>true</b> if an entity is of any of the given types; othewise <b>false</b>.</returns>
        public static bool Is(this IEntity entity, EntityId type)
        {
            return entity.Is(new[] { type });
        }

        /// <summary>Determines if a given entity is of any of the types provided.</summary>
        /// <param name="entity">Entity to operate on.</param>
        /// <param name="types">Enumeration of types to check against.</param>
        /// <returns><b>true</b> if an entity is of any of the given types; othewise <b>false</b>.</returns>
        public static bool Is(this IEntity entity, IEnumerable<EntityId> types)
        {
            return ((entity != null) && (types != null) && entity.GetTypes().Join(types, item => item, item => item, (left, right) => left).Any());
        }

        /// <summary>Gets an enumeration of all entity predicats that are currently set.</summary>
        /// <param name="entity">Entity for which predicates will be gathered.</param>
        /// <returns>Enumeration of predicate Uri's.</returns>
        public static IEnumerable<Uri> Predicates(this IEntity entity)
        {
            IEnumerable<Uri> result = new Uri[0];
            if (entity != null)
            {
                result = entity.Context.Store.Quads.WhereQuadDescribesEntity(entity).Select(item => item.Predicate.Uri);
            }

            return result;
        }

        /// <summary>Gets the value of the given predicate.</summary>
        /// <param name="entity">Entity for which the value should be get.</param>
        /// <param name="predicate">Uri of the predicate the value should be get.</param>
        /// <remarks>This method returns strongly typed values as defined in the mappings.</remarks>
        /// <returns>Value of the given predicate or <b>null</b>.</returns>
        public static object Predicate(this IEntity entity, Uri predicate)
        {
            object result;
            IPropertyMapping propertyMapping = entity.Context.Mappings.MappingForProperty(predicate);
            if (propertyMapping != null)
            {
                var mappedEntity = typeof(EntityExtensions).GetTypeInfo()
                    .GetMethod("AsEntity").MakeGenericMethod(propertyMapping.EntityMapping.EntityType)
                    .Invoke(null, new object[] { entity });
                result = mappedEntity.GetType().GetTypeInfo().GetProperty(propertyMapping.Name).GetValue(mappedEntity);
            }
            else
            {
                ICollection<object> output = new List<object>();
                IEnumerable<INode> objects =
                    entity.Context.Store.Quads.WhereQuadDescribesEntity(entity)
                          .Where(item => item.Predicate.Uri.AbsoluteUri == predicate.AbsoluteUri)
                          .Select(item => item.Object);
                foreach (Node @object in objects)
                {
                    if ((@object.IsUri) || (@object.IsBlank))
                    {
                        output.Add(entity.Context.Load<IEntity>(@object.ToEntityId()));
                    }
                    else
                    {
                        object item = entity.Context.FallbackNodeConverter.Convert(@object, entity.Context);
                        if (item != null)
                        {
                            output.Add(item);
                        }
                    }
                }

                result = output;
            }

            return result;
        }

        /// <summary>Forces lazy initialization of <paramref name="entity"/>.</summary>
        public static void ForceInitialize(this IEntity entity)
        {
            entity = UnwrapProxy(entity);

            if (entity is Entity)
            {
                ((Entity)entity).EnsureIsInitialized();
            }
            else if (entity is EntityProxy)
            {
                ((EntityProxy)entity).AsDynamic().EnsureIsInitialized();
            }
        }

        /// <summary>Creates a blank identifier, which will be associated with this entity.</summary>
        /// <param name="entity">The root entity.</param>
        public static BlankId CreateBlankId(this IEntity entity)
        {
            var blankIdGenerator = entity.Context.BlankIdGenerator;
            return new BlankId(blankIdGenerator.Generate(), entity.Id);
        }

        internal static IEntity UnwrapProxy(this IEntity entity)
        {
            var result = entity as IProxy;
            if (result != null)
            {
                return (IEntity)result.WrappedObject;
            }

            return entity;
        }

        private static IEnumerable<IEntityQuad> WhereQuadDescribesEntity(this IEnumerable<IEntityQuad> quads, IEntity entity)
        {
            return quads.Where(item => (item.EntityId == entity.Id) &&
                (((item.Subject.IsUri) && (!(entity.Id is BlankId)) && (item.Subject.Uri.AbsoluteUri == entity.Id.Uri.AbsoluteUri)) ||
                ((item.Subject.IsBlank) && (entity.Id is BlankId) && (item.Subject.BlankNode == ((BlankId)entity.Id).Identifier))));
        }
    }
}