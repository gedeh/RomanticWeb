using System;
using RomanticWeb.Diagnostics;
using RomanticWeb.Mapping.Visitors;

namespace RomanticWeb.Mapping.Providers
{
    /// <summary>Mapping provider for RDF class.</summary>
    public class ClassMappingProvider : TermMappingProviderBase, IClassMappingProvider
    {
        private readonly Type _entityType;

        /// <summary>Initializes a new instance of the <see cref="ClassMappingProvider" /> class.</summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="log">Logging facility</param>
        public ClassMappingProvider(Type entityType, Uri uri, ILogger log) : base(uri, log)
        {
            _entityType = entityType;
        }

        /// <summary>Initializes a new instance of the <see cref="ClassMappingProvider" /> class.</summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="prefix">The QName prefix.</param>
        /// <param name="term">The QName term.</param>
        /// <param name="log">Logging facility</param>
        public ClassMappingProvider(Type entityType, string prefix, string term, ILogger log) : base(prefix, term, log)
        {
            _entityType = entityType;
        }

        /// <inheritdoc/>
        public Type DeclaringEntityType { get { return _entityType; } }

        /// <inheritdoc/>
        public override void Accept(IMappingProviderVisitor mappingProviderVisitor)
        {
            mappingProviderVisitor.Visit(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("Class mapping");
        }
    }
}