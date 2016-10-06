using System;
using System.Collections.Generic;
using RomanticWeb.Linq.Model;
using RomanticWeb.Linq.Sparql;

namespace RomanticWeb.DotNetRDF
{
    /// <summary>Parses given query into a SPARQL 1.1 query.</summary>
    [Obsolete("This class is only to address issue with donNetRDF SPARQL algebra that generates instable result sets. Please use the GenericSparqlQueryVisitor from the RomanticWeb assembly.")]
    public class SparqlQueryVisitor : GenericSparqlQueryVisitor
    {
        private readonly IList<string> _identifiersAccessingMetaGraph;

        internal SparqlQueryVisitor(IList<string> identifiersAccessingMetaGraph)
        {
            _identifiersAccessingMetaGraph = identifiersAccessingMetaGraph;
        }

        internal IEnumerable<string> IdentifiersAccessingMetaGraph { get { return _identifiersAccessingMetaGraph; } }

        /// <inheritdoc />
        protected override void VisitStrongEntityAccessorGraph(StrongEntityAccessor entityAccessor)
        {
            if (_identifiersAccessingMetaGraph == null)
            {
                base.VisitStrongEntityAccessorGraph(entityAccessor);
            }
            else if (!_identifiersAccessingMetaGraph.Contains(entityAccessor.About.Name))
            {
                _identifiersAccessingMetaGraph.Add(entityAccessor.About.Name);
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