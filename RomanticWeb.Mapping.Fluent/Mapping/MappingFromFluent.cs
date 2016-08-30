using System.Reflection;
using RomanticWeb.Diagnostics;
using RomanticWeb.Mapping.Sources;

namespace RomanticWeb.Mapping
{
    internal class MappingFromFluent : IMappingFrom
    {
        private readonly ILogger _log;

        /// <summary>Initializes a new instance of the <see cref="MappingFromFluent" /> class.</summary>
        /// <param name="log">Logging facility.</param>
        public MappingFromFluent(ILogger log)
        {
            _log = log;
        }

        /// <inheritdoc />
        public void FromAssemblyOf<T>(IMappingBuilder builder)
        {
            FromAssembly(builder, typeof(T).Assembly);
        }

        /// <inheritdoc />
        public void FromAssembly(IMappingBuilder builder, Assembly assembly)
        {
            builder.AddMapping(assembly, new FluentMappingsSource(assembly, _log));
        }
    }
}