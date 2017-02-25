using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private const int QueryOptimizationTriplesCountThreshold = 1000000;
        private const long Timeout = 2000;
        private readonly LeviathanQueryProcessor _processor;

        /// <summary>Initializes a new instance of the <see cref="InMemoryTripleStoreSparqlCommandExecutionStrategy" /> class.</summary>
        /// <param name="store">Target in-memory triple store.</param>
        /// <param name="metaGraphUri">Meta-graph uri.</param>
        public InMemoryTripleStoreSparqlCommandExecutionStrategy(IInMemoryQueryableStore store, Uri metaGraphUri) : base(store, metaGraphUri)
        {
            _processor = new LeviathanQueryProcessor(new InMemoryQuadDataset((IInMemoryQueryableStore)Store, MetaGraphUri));
        }

        /// <inheritdoc />
        public override bool ExecuteAsk(IQuery sparqlQuery)
        {
            SparqlQueryVariables variables;
            return ((SparqlResultSet)_processor.ProcessQuery(CreateSparqlQuery(sparqlQuery, out variables))).Result;
        }

        /// <inheritdoc />
        public override SparqlResultSet ExecuteSelect(SparqlQuery sparqlQuery)
        {
            return (SparqlResultSet)_processor.ProcessQuery(sparqlQuery);
        }

        /// <inheritdoc />
        public override SparqlResultSet ExecuteSelect(IQuery sparqlQuery, out SparqlQueryVariables variables)
        {
            bool isQueryOptimized = Math.Pow(Store.Triples.Count(), 2) > QueryOptimizationTriplesCountThreshold;
            string commandText = ParseQuery(sparqlQuery, out variables, ref isQueryOptimized);
            var parser = new SparqlQueryParser();
            if (!isQueryOptimized)
            {
                return (SparqlResultSet)_processor.ProcessQuery(parser.ParseFromString(commandText));
            }

            return ExecuteOptimizedSelect(commandText, variables, parser);
        }

        private SparqlResultSet ExecuteOptimizedSelect(string commandText, SparqlQueryVariables variables, SparqlQueryParser parser)
        {
            var parametrizedCommandText = new SparqlParameterizedString(commandText);
            var metaGraphQuery = new SparqlQueryParser().ParseFromString(
                String.Format(
                    "SELECT ?{0} WHERE {{ GRAPH <{2}> {{ ?{0} <http://xmlns.com/foaf/0.1/primaryTopic> ?{1} . }} }}",
                    variables.MetaGraph,
                    variables.Entity,
                    MetaGraphUri));
            IEnumerable<SparqlResult> result = new List<SparqlResult>();
            SparqlQueryVariables queryVariables = variables;
            Parallel.ForEach(
                ((SparqlResultSet)_processor.ProcessQuery(metaGraphQuery)).Results,
                (row, state) => ProcessPartialResult(parametrizedCommandText, queryVariables, parser, (List<SparqlResult>)result, row, state));
            return new SparqlResultSet(result);
        }

        private void ProcessPartialResult(
            SparqlParameterizedString parametrizedCommandText,
            SparqlQueryVariables queryVariables,
            SparqlQueryParser parser,
            List<SparqlResult> result,
            SparqlResult row,
            ParallelLoopState state)
        {
            if (state.IsStopped)
            {
                return;
            }

            SparqlQuery query;
            lock (parametrizedCommandText)
            {
                parametrizedCommandText.SetUri("graph", ((IUriNode)row[queryVariables.MetaGraph]).Uri);
                query = parser.ParseFromString(parametrizedCommandText);
            }

            query.Timeout = Timeout;
            try
            {
                var graphResult = ((SparqlResultSet)_processor.ProcessQuery(query)).Results;
                lock (result)
                {
                    result.AddRange(graphResult);
                }
            }
            catch (RdfQueryTimeoutException)
            {
                state.Stop();
                lock (result)
                {
                    result.Clear();
                }
            }
        }

        private SparqlQuery CreateSparqlQuery(IQuery sparqlQuery, out SparqlQueryVariables variables)
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            bool isQueryOptimized = false;
            return parser.ParseFromString(ParseQuery(sparqlQuery, out variables, ref isQueryOptimized));
        }

        private string ParseQuery(IQuery sparqlQuery, out SparqlQueryVariables variables, ref bool isQueryOptimized)
        {
            var queryVisitor = new SparqlQueryVisitor(isQueryOptimized);
            queryVisitor.MetaGraphUri = MetaGraphUri;
            queryVisitor.VisitQuery(sparqlQuery);
            variables = queryVisitor.Variables;
            isQueryOptimized = queryVisitor.IsQueryOptimized;
            return queryVisitor.CommandText;
        }
    }
}
