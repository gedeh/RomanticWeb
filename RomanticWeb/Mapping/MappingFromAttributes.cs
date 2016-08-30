using System.Reflection;
using RomanticWeb.Diagnostics;
using RomanticWeb.Mapping.Sources;

namespace RomanticWeb.Mapping
{
    internal class MappingFromAttributes : IMappingFrom
    {
        private readonly ILogger _log;

        /// <summary>Initializes a new instance of the <see cref="MappingFromAttributes" /> class.</summary>
        /// <param name="log">Logging facility.</param>
        public MappingFromAttributes(ILogger log)
        {
            _log = log;
        }

        /// <inheritdoc />
        public void FromAssemblyOf<T>(IMappingBuilder builder)
        {
            FromAssembly(builder, typeof(T).Assembly);
        }

        /// <inheritdoc />
        public void FromAssembly(IMappingBuilder builder, Assembly mappingAssembly)
        {
            builder.AddMapping(mappingAssembly, new AttributeMappingsSource(mappingAssembly, _log));
        }
    }
}