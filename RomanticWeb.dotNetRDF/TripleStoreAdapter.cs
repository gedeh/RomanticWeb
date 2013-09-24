﻿using System;
using System.Collections.Generic;
using RomanticWeb.DotNetRDF.TripleSources;
using RomanticWeb.Entities;
using RomanticWeb.Ontologies;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Datasets;

namespace RomanticWeb.DotNetRDF
{
    public class TripleStoreAdapter:ITripleStoreAdapter
    {
        private readonly TripleStore _store;

        private readonly INamespaceMapper _namespaces;

        public TripleStoreAdapter(TripleStore store)
        {
            _store=store;
            _namespaces = new NamespaceMapper(true);
            _namespaces.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
        }

        public IEnumerable<Tuple<RdfNode,RdfNode,RdfNode>> GetNodesForQuery(string sparqlConstruct)
        {
            throw new NotImplementedException();
        }

        public void LoadEntity(IEntityStore store,EntityId entityId)
        {
            var select = QueryBuilder.Select("s", "p", "o", "g")
                                     .Graph("?g", g => g.Where(t => t.Subject("s").Predicate("p").Object("o")))
                                     .Where(t => t.Subject("g").PredicateUri("foaf:primaryTopic").Object(entityId.Uri));
            select.Prefixes.Import(_namespaces);

            var processor = new LeviathanQueryProcessor(new InMemoryQuadDataset(_store, new Uri("http://app.magi/graphs")));
            foreach (var result in (SparqlResultSet)processor.ProcessQuery(select.BuildQuery()))
            {
                RdfNode subject = result["s"].WrapNode();
                RdfNode predicate = result["p"].WrapNode();
                RdfNode @object=result["o"].WrapNode();
                RdfNode graph=result.HasBoundValue("g")?result["g"].WrapNode():null;
                store.AssertTriple(Tuple.Create(subject, predicate, @object, graph));
            }
        }
    }
}