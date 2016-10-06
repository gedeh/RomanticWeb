using System;
using System.Collections;
using System.Collections.Generic;
using RomanticWeb.Linq.Model;
using RomanticWeb.Linq.Sparql;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace RomanticWeb.DotNetRDF
{
    /// <summary>Provides a default implementation of the <see cref="ISparqlCommandExecutionStrategy" /> for in-memory triple stores.</summary>
    public class InMemoryTripleStoreSparqlCommandExecutionStrategy : PersistentTripleStoreSparqlCommandExecutionStrategy
    {
        /// <summary>Initializes a new instance of the <see cref="InMemoryTripleStoreSparqlCommandExecutionStrategy" /> class.</summary>
        /// <param name="store">Target in-memory triple store.</param>
        /// <param name="metaGraphUri">Meta-graph uri.</param>
        public InMemoryTripleStoreSparqlCommandExecutionStrategy(IInMemoryQueryableStore store, Uri metaGraphUri) : base(store, metaGraphUri)
        {
        }

        /// <inheritdoc />
        public override SparqlQuery GetSparqlQuery(IQuery sparqlQuery, out SparqlQueryVariables variables)
        {
            return GetSparqlQuery(sparqlQuery, null, out variables);
        }

        /// <inheritdoc />
        public override bool ExecuteAsk(IQuery sparqlQuery)
        {
            var inMemoryQuadDataset = new InMemoryQuadDataset((IInMemoryQueryableStore)Store, MetaGraphUri);
            var processor = new LeviathanQueryProcessor(inMemoryQuadDataset);
            SparqlQueryVariables variables;
            return ((SparqlResultSet)processor.ProcessQuery(GetSparqlQuery(sparqlQuery, out variables))).Result;
        }

        /// <inheritdoc />
        public override SparqlResultSet ExecuteSelect(SparqlQuery sparqlQuery)
        {
            var inMemoryQuadDataset = new InMemoryQuadDataset((IInMemoryQueryableStore)Store, MetaGraphUri);
            var processor = new LeviathanQueryProcessor(inMemoryQuadDataset);
            return (SparqlResultSet)processor.ProcessQuery(sparqlQuery);
        }

        /// <inheritdoc />
        public override SparqlResultSet ExecuteSelect(IQuery sparqlQuery, out SparqlQueryVariables variables)
        {
            var query = GetSparqlQuery(sparqlQuery, null, out variables);
            var result = ExecuteSelect(query);
            return result;
        }

        /// <inheritdoc />
        private SparqlQuery GetSparqlQuery(IQuery sparqlQuery, IList<string> identifiersAccessingMetaGraph, out SparqlQueryVariables variables)
        {
            GenericSparqlQueryVisitor queryVisitor = new SparqlQueryVisitor(identifiersAccessingMetaGraph);
            queryVisitor.MetaGraphUri = MetaGraphUri;
            queryVisitor.VisitQuery(sparqlQuery);
            variables = queryVisitor.Variables;
            SparqlQueryParser parser = new SparqlQueryParser();
            return parser.ParseFromString(queryVisitor.CommandText);
        }
    }
}