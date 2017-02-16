using System;
using System.Collections.Generic;
using System.Linq;
using RomanticWeb.Linq.Model;
using RomanticWeb.Linq.Sparql;

namespace RomanticWeb.DotNetRDF
{
    /// <summary>Parses given query into a SPARQL 1.1 query.</summary>
    /// <remarks>
    /// This whole "construct" is (hopefuly) temporary workaround for poor performance of SPARQL queries on in-memory triple stores.
    /// In general, having a query of two graph patterns is processed as a cross-product. For stores with thousends triples this will end up with a timeout.
    /// For some simple cases, it is possible to convert the query to a set of queries executed against specific graphs, thus overal processing can be finished in timely fashion. 
    /// </remarks>
    [Obsolete("This class is only to address issue with donNetRDF SPARQL algebra that generates instable result sets. Please use the GenericSparqlQueryVisitor from the RomanticWeb assembly.")]
    public class SparqlQueryVisitor : GenericSparqlQueryVisitor
    {
        private bool _shouldOverrideMetaGraphQuery = true;

        /// <summary>Initializes a new instance of the <see cref="SparqlQueryVisitor"/> class.</summary>
        /// <param name="requiresQueryOptimizations">Instructs the visitor to optimize query for in-memory dotNetRDF processing.</param>
        public SparqlQueryVisitor(bool requiresQueryOptimizations)
        {
            _shouldOverrideMetaGraphQuery = requiresQueryOptimizations;
        }

        /// <summary>Gets a value indicating whether this query optimized.</summary>
        public bool IsQueryOptimized { get { return _shouldOverrideMetaGraphQuery; } }

        /// <inheritdoc />
        public override void VisitQuery(IQuery query)
        {
            if ((query.OrderBy.Count > 0) || (query.Limit > 0) || (query.Offset > 0))
            {
                _shouldOverrideMetaGraphQuery = false;
            }

            base.VisitQuery(query);
        }

        /// <inheritdoc />
        protected override void VisitStrongEntityAccessorGraph(StrongEntityAccessor entityAccessor)
        {
            if (entityAccessor.UnboundGraphName != null)
            {
                return;
            }

            if (!_shouldOverrideMetaGraphQuery)
            {
                base.VisitStrongEntityAccessorGraph(entityAccessor);
                return;
            }

            CommandTextBuilder.Append(Indentation);
            CommandTextBuilder.AppendFormat("BIND(@graph AS ?G{0}) ", entityAccessor.About.Name);
            CommandTextBuilder.AppendLine();
        }

        /// <inheritdoc />
        protected override void VisitCall(Call call)
        {
            base.VisitCall(call);
            if (call.Member == MethodNames.Count)
            {
                _shouldOverrideMetaGraphQuery = false;
            }
        }

        /// <inheritdoc />
        protected override void VisitQueryResultModifiers(IDictionary<IExpression, bool> orderByExpressions, int offset, int limit)
        {
            if (orderByExpressions.Count == 0)
            {
                CommandTextBuilder.Append("ORDER BY ");
                VisitComponent(CurrentEntityAccessor.About);
                CommandTextBuilder.Append(" ");
            }

            base.VisitQueryResultModifiers(orderByExpressions, offset, limit);
        }
    }
}