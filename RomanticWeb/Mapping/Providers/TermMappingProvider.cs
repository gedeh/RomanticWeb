using System;
using RomanticWeb.Diagnostics;
using RomanticWeb.Mapping.Visitors;
using RomanticWeb.Ontologies;

namespace RomanticWeb.Mapping.Providers
{
    /// <summary>Base class for mapping providers, which return a RDF term mapping.</summary>
    public abstract class TermMappingProviderBase : ITermMappingProvider
    {
        private readonly ILogger _log;

        /// <summary>Initializes a new instance of the <see cref="TermMappingProviderBase"/> class.</summary>
        /// <param name="termUri">The term URI.</param>
        /// <param name="log">Logging facility.</param>
        protected TermMappingProviderBase(Uri termUri, ILogger log) : this(log)
        {
            ((ITermMappingProvider)this).GetTerm = provider => termUri;
        }

        /// <summary>Initializes a new instance of the <see cref="TermMappingProviderBase"/> class.</summary>
        /// <param name="namespacePrefix">The namespace prefix.</param>
        /// <param name="term">The term.</param>
        /// <param name="log">Logging facility.</param>
        protected TermMappingProviderBase(string namespacePrefix, string term, ILogger log) : this(log)
        {
            ((ITermMappingProvider)this).GetTerm = provider => GetTermUri(provider, namespacePrefix, term, _log);
        }

        /// <summary>Initializes an empty <see cref="TermMappingProviderBase"/>.</summary>
        /// <param name="log">Logging facility.</param>
        protected TermMappingProviderBase(ILogger log)
        {
            _log = log;
        }

        Func<IOntologyProvider, Uri> ITermMappingProvider.GetTerm { get; set; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="mappingProviderVisitor">The visitor.</param>
        public abstract void Accept(IMappingProviderVisitor mappingProviderVisitor);

        // TODO: Consider a mechanism of ignoring missing ontology providers. Use case: partial publications, i.e. updated assembly with new mappings, but not published prefix mapping.
        private static Uri GetTermUri(IOntologyProvider ontologyProvider, string namespacePrefix, string termName, ILogger log)
        {
            var resolvedUri = ontologyProvider.ResolveUri(namespacePrefix, termName);

            if (resolvedUri == null)
            {
                var message = string.Format("Cannot resolve QName {0}:{1}", namespacePrefix, termName);
                log.Fatal(message);
                throw new MappingException(message);
            }

            return resolvedUri;
        }
    }
}