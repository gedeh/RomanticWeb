using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using RomanticWeb.Entities.Proxies;
using RomanticWeb.Entities.ResultAggregations;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Model;

namespace RomanticWeb.Entities.ResultPostprocessing
{
    /// <summary>Transforms RDF object values to an <see cref="ObservableCollection{T}"/>.</summary>
    public class ObservableCollectionTransformer : SimpleTransformer
    {
        private static readonly MethodInfo EnumerableCast = typeof(Enumerable).GetMethod("Cast");
        private static readonly MethodInfo AsEntity = typeof(EntityExtensions).GetMethod("AsEntity");

        /// <summary>Initializes a new instance of the <see cref="ObservableCollectionTransformer"/> class.</summary>
        public ObservableCollectionTransformer() : base(new OriginalResult())
        {
        }

        /// <summary>Get an <see cref="ObservableCollection{T}"/> containing <paramref name="nodes"/>' values.</summary>
        public override object FromNodes(IEntityProxy parent, IPropertyMapping property, IEntityContext context, IEnumerable<INode> nodes)
        {
            var convertedValues = nodes.Select(node => ((ICollectionMapping)property).ElementConverter.Convert(node, context));
            var collectionElements = ((IEnumerable<object>)Aggregator.Aggregate(convertedValues)).ToArray();
            var genericArguments = (property.ReturnType.IsArray ? new[] { property.ReturnType.GetElementType() } : property.ReturnType.GetGenericArguments());
            var observable = (IList)typeof(ObservableCollection<>).MakeGenericType(genericArguments).GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
            var asEntity = (typeof(IEntity).IsAssignableFrom(genericArguments.Single()) ? AsEntity.MakeGenericMethod(genericArguments) : null);
            foreach (var item in collectionElements)
            {
                observable.Add(asEntity != null ? asEntity.Invoke(null, new[] { item }) : item);
            }

            ((INotifyCollectionChanged)observable).CollectionChanged += (sender, args) => DynamicExtensions.InvokeSet((dynamic)parent, property.Name, sender);
            return observable;
        }

        /// <summary>Gets a node for each collection element.</summary>
        public override IEnumerable<INode> ToNodes(object collection, IEntityProxy proxy, IPropertyMapping property, IEntityContext context)
        {
            return from object value in (IEnumerable)collection
                   select base.ToNodes(value, proxy, property, context).Single();
        }

        /// <inheritdoc />
        protected override object Transform(INode node, IPropertyMapping property, IEntityContext context)
        {
            return property.Converter.Convert(node, context);
        }
    }
}