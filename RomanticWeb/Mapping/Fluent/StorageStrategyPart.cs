﻿using RomanticWeb.Entities.ResultAggregations;
using RomanticWeb.Mapping.Model;

namespace RomanticWeb.Mapping.Fluent
{
    /// <summary>
    /// Allows setting the way in which collections are persisted as triples
    /// </summary>
    public class StorageStrategyPart
    {
        private readonly CollectionMap _collectionMap;

        internal StorageStrategyPart(CollectionMap collectionMap)
        {
            _collectionMap=collectionMap;
        }

        /// <summary>
        /// Marks a collection to be stored as a set of triples
        /// </summary>
        public CollectionMap SimpleCollection()
        {
            _collectionMap.StorageStrategy=StorageStrategyOption.Simple;
            _collectionMap.Aggregation=Aggregation.Original;
            return _collectionMap;
        }

        /// <summary>
        /// Marks a collection to be stored as an rfd:List
        /// </summary>
        public CollectionMap RdfList()
        {
            _collectionMap.StorageStrategy=StorageStrategyOption.RdfList;
            _collectionMap.Aggregation=Aggregation.SingleOrDefault;
            return _collectionMap;
        }
    }
}