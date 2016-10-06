using System;
using VDS.RDF;

namespace RomanticWeb.DotNetRDF
{
    /// <summary>Provides an abstract SPARQl command execution strategy factory.</summary>
    public interface ISparqlCommandExecutionStrategyFactory
    {
        /// <summary>Creates an instance of <see cref="ISparqlCommandExecutionStrategy" /> for a given <see cref="ITripleStore" />.</summary>
        /// <param name="store">Target triple store for which to create a SPARQL command execution strategy.</param>
        /// <param name="metaGraphUri">Meta-graph Uri.</param>
        /// <returns>Instance of <see cref="ISparqlCommandExecutionStrategy" />.</returns>
        ISparqlCommandExecutionStrategy CreateFor(ITripleStore store, Uri metaGraphUri);
    }
}