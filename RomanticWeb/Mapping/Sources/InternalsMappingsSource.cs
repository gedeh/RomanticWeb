using System.Collections.Generic;
using System.Reflection;
using RomanticWeb.Collections;
using RomanticWeb.Converters;
using RomanticWeb.Mapping.Providers;
using RomanticWeb.Vocabularies;

namespace RomanticWeb.Mapping.Sources
{
    /// <summary>Mappings repository, which generates mappings for some internal types.</summary>
    public sealed class InternalsMappingsSource : AssemblyMappingsSource
    {
        /// <summary>Creates a new instance of <see cref="AttributeMappingsSource"/>.</summary>
        public InternalsMappingsSource(Assembly assembly) : base(assembly)
        {
        }

        /// <inheritdoc />
        public override string Description { get { return string.Format("Internals mappings from assembly {0}", Assembly); } }

        /// <summary>Create mapping propviders from mapping attributes.</summary>
        public override IEnumerable<IEntityMappingProvider> GetMappingProviders()
        {
            var type = typeof(IRdfListNode<object>);
            var restMapping = new PropertyMappingProvider(Rdf.rest, type.GetProperty("Rest"), null);
            restMapping.ConverterType = typeof(AsEntityConverter<IRdfListNode<object>>);
            var firstMapping = new PropertyMappingProvider(Rdf.first, type.GetProperty("First"), null);
            firstMapping.ConverterType = typeof(FallbackNodeConverter);
            yield return new EntityMappingProvider(typeof(IRdfListNode<object>), new IClassMappingProvider[0], new[] { restMapping, firstMapping });
        }
    }
}