using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RomanticWeb.Diagnostics;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using RomanticWeb.Mapping.Providers;

namespace RomanticWeb.Mapping.Sources
{
    /// <summary>Mappings repository, which reads mapping attributes from an assembly.</summary>
    public sealed class AttributeMappingsSource : AssemblyMappingsSource
    {
        private readonly ILogger _log;

        /// <summary>Creates a new instance of <see cref="AttributeMappingsSource"/>.</summary>
        public AttributeMappingsSource(Assembly assembly, ILogger log) : base(assembly)
        {
            _log = log;
        }

        /// <inheritdoc />
        public override string Description { get { return string.Format("Attribute mappings from assembly {0}", Assembly); } }

        /// <summary>Create mapping propviders from mapping attributes.</summary>
        public override IEnumerable<IEntityMappingProvider> GetMappingProviders()
        {
            var builder = new AttributeMappingProviderBuilder(_log);
            return from type in Assembly.GetTypesWhere(type => typeof(IEntity).IsAssignableFrom(type))
                   select builder.Visit(type);
        }
    }
}