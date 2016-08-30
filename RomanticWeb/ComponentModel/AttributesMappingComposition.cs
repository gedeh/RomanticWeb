using RomanticWeb.Mapping;

namespace RomanticWeb.ComponentModel
{
    internal sealed class AttributesMappingComposition : CompositionRootBase
    {
        public AttributesMappingComposition()
        {
            MappingFrom<MappingFromAttributes>();
        }
    }
}