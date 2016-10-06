using System;
using RomanticWeb.Linq.Model;
using RomanticWeb.Linq.Sparql;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace RomanticWeb.DotNetRDF
{
    /// <summary>Provides a default implementation of the <see cref="ISparqlCommandExecutionStrategy" /> for peristent triple stores.</summary>
    public class PersistentTripleStoreSparqlCommandExecutionStrategy : ISparqlCommandExecutionStrategy
    {
        /// <summary>Initializes a new instance of the <see cref="PersistentTripleStoreSparqlCommandExecutionStrategy" /> class.</summary>
        /// <param name="store">Target in-memory triple store.</param>
        /// <param name="metaGraphUri">Meta-graph uri.</param>
        public PersistentTripleStoreSparqlCommandExecutionStrategy(ITripleStore store, Uri metaGraphUri)
        {
            MetaGraphUri = metaGraphUri;
            Store = store;
        }

        /// <summary>Gets a meta-graph Uri.</summary>
        protected Uri MetaGraphUri { get; private set; }

        /// <summary>Gets a triple store.</summary>
        protected ITripleStore Store { get; private set; }

        /// <inheritdoc />
        public SparqlQuery GetSparqlQuery(IQuery sparqlQuery)
        {
            SparqlQueryVariables variables;
            return GetSparqlQuery(sparqlQuery, out variables);
        }

        /// <inheritdoc />
        public virtual SparqlQuery GetSparqlQuery(IQuery sparqlQuery, out SparqlQueryVariables variables)
        {
            GenericSparqlQueryVisitor queryVisitor = new GenericSparqlQueryVisitor() { MetaGraphUri = MetaGraphUri };
            queryVisitor.VisitQuery(sparqlQuery);
            variables = queryVisitor.Variables;
            SparqlQueryParser parser = new SparqlQueryParser();
            return parser.ParseFromString(queryVisitor.CommandText);
        }

        /// <inheritdoc />
        public virtual void ExecuteCommandSet(SparqlUpdateCommandSet commands)
        {
            var store = Store as IUpdateableTripleStore;
            if (store == null)
            {
                throw new InvalidOperationException(String.Format("Store doesn't implement {0}", typeof(IUpdateableTripleStore)));
            }

            store.ExecuteUpdate(commands);
        }

        /// <inheritdoc />
        public virtual bool ExecuteAsk(IQuery sparqlQuery)
        {
            SparqlQueryVariables variables;
            return ((SparqlResultSet)((INativelyQueryableStore)Store).ExecuteQuery(GetSparqlQuery(sparqlQuery, out variables).ToString())).Result;
        }

        /// <inheritdoc />
        public virtual bool ExecuteAsk(SparqlQuery sparqlQuery)
        {
            return ((SparqlResultSet)((INativelyQueryableStore)Store).ExecuteQuery(sparqlQuery.ToString())).Result;
        }

        /// <inheritdoc />
        public virtual SparqlResultSet ExecuteSelect(SparqlQuery sparqlQuery)
        {
            return (SparqlResultSet)((INativelyQueryableStore)Store).ExecuteQuery(sparqlQuery.ToString());
        }

        /// <inheritdoc />
        public SparqlResultSet ExecuteSelect(IQuery sparqlQuery)
        {
            SparqlQueryVariables variables;
            return ExecuteSelect(sparqlQuery, out variables);
        }

        /// <inheritdoc />
        public virtual SparqlResultSet ExecuteSelect(IQuery sparqlQuery, out SparqlQueryVariables variables)
        {
            return (SparqlResultSet)((INativelyQueryableStore)Store).ExecuteQuery(GetSparqlQuery(sparqlQuery, out variables).ToString());
        }
    }
}