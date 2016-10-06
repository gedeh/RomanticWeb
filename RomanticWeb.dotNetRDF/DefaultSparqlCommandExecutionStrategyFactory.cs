using System;
using VDS.RDF;

namespace RomanticWeb.DotNetRDF
{
    /// <summary>Provides a default implementation of the <see cref="ISparqlCommandExecutionStrategyFactory" />.</summary>
    public class DefaultSparqlCommandExecutionStrategyFactory : ISparqlCommandExecutionStrategyFactory
    {
        /// <inheritdoc />
        public ISparqlCommandExecutionStrategy CreateFor(ITripleStore store, Uri metaGraphUri)
        {
            var inMemoryStore = store as IInMemoryQueryableStore;
            if (inMemoryStore != null)
            {
                return new InMemoryTripleStoreSparqlCommandExecutionStrategy(inMemoryStore, metaGraphUri);
            }

            return new PersistentTripleStoreSparqlCommandExecutionStrategy(store, metaGraphUri);
        }
    }
}