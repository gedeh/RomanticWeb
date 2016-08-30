using System;
using RomanticWeb.Diagnostics;
using RomanticWeb.Mapping.Visitors;

namespace RomanticWeb.Mapping.Providers
{
    /// <summary>
    /// Mapping provider, which returns a mapping for dictionary key predicate
    /// </summary>
    public class KeyMappingProvider : TermMappingProviderBase, IPredicateMappingProvider
    {
        /// <summary>Initializes a new instance of the <see cref="KeyMappingProvider"/> class.</summary>
        /// <param name="termUri">The term URI.</param>
        /// <param name="log">Logging facility.</param>
        public KeyMappingProvider(Uri termUri, ILogger log) : base(termUri, log)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="KeyMappingProvider"/> class.</summary>
        /// <param name="namespacePrefix">The namespace prefix.</param>
        /// <param name="term">The term.</param>
        /// <param name="log">Logging facility.</param>
        public KeyMappingProvider(string namespacePrefix, string term, ILogger log) : base(namespacePrefix, term, log)
        {
        }

        /// <summary>Initializes an empty <see cref="KeyMappingProvider"/>.</summary>
        /// <param name="log">Logging facility.</param>
        public KeyMappingProvider(ILogger log) : base(log)
        {
        }

        /// <inheritdoc/>
        public Type ConverterType { get; set; }

        /// <summary>Does nothing.</summary>
        public override void Accept(IMappingProviderVisitor mappingProviderVisitor)
        {
        }
    }
}