using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Sources;
using RomanticWeb.Mapping.Visitors;

namespace RomanticWeb.ComponentModel
{
    internal sealed class FluentMappingComposition : CompositionRootBase
    {
        public FluentMappingComposition()
        {
            MappingProviderVisitor<GeneratedListMappingSource>();
            MappingProviderVisitor<GeneratedDictionaryMappingSource>();
            MappingFrom<MappingFromFluent>();
            DefaultMappings();
        }

        private void DefaultMappings()
        {
            var mappingBuilder = new MappingBuilder(new[] { new MappingFromFluent(null) });
            mappingBuilder.FromAssemblyOf<IFluentMapsVisitor>();
            foreach (var source in mappingBuilder.Sources)
            {
                SharedComponent(source, source.Description);
            }
        }
    }
}