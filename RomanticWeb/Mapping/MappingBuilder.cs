using System.Collections.Generic;
using System.Reflection;
using RomanticWeb.Mapping.Sources;

namespace RomanticWeb.Mapping
{
    /// <summary>Builder for registering mapping repositories with <see cref="IEntityContextFactory"/>.</summary>
    public sealed class MappingBuilder : IMappingBuilder
    {
        private readonly IList<IMappingProviderSource> _sources = new List<IMappingProviderSource>();
        private readonly IEnumerable<IMappingFrom> _mappingFrom;

        /// <summary>Initializes a new instance if the <see cref="MappingBuilder" /> class.</summary>
        /// <param name="mappingFrom">Registered mapping sources.</param>
        public MappingBuilder(IEnumerable<IMappingFrom> mappingFrom)
        {
            _mappingFrom = mappingFrom;
        }

        internal IEnumerable<IMappingProviderSource> Sources { get { return _sources; } }

        /// <summary>Registers both fluent and attrbiute mappings from an assembly.</summary>
        public void FromAssemblyOf<T>()
        {
            foreach (var from in _mappingFrom)
            {
                from.FromAssemblyOf<T>(this);
            }
        }

        /// <summary>Registers both fluent and attrbiute mappings from an assembly.</summary>
        public void FromAssembly(Assembly assembly)
        {
            foreach (var from in _mappingFrom)
            {
                from.FromAssembly(this, assembly);
            }
        }

        /// <summary>Adds a given mapping.</summary>
        /// <typeparam name="TMappingRepository">Type of the mapping repository.</typeparam>
        /// <param name="mappingAssembly">Source assembly.</param>
        /// <param name="mappingProvider">Mappings provider.</param>
        public void AddMapping<TMappingRepository>(Assembly mappingAssembly, TMappingRepository mappingProvider) where TMappingRepository : IMappingProviderSource
        {
            _sources.Add(mappingProvider);
        }
    }
}