using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Conventions;
using RomanticWeb.Mapping.Validation;
using RomanticWeb.Mapping.Visitors;

namespace RomanticWeb.ComponentModel
{
    internal sealed class MappingComposition : CompositionRootBase
    {
        public MappingComposition()
        {
            RdfTypeCache<RdfTypeCache>();
            MappingModelVisitor<RdfTypeCacheBuilder>();
            MappingProviderVisitor<ConventionsVisitor>();
            MappingProviderVisitor<MappingProvidersValidator>();
        }
    }
}