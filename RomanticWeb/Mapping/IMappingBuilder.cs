using System.Reflection;
using RomanticWeb.Mapping.Sources;

namespace RomanticWeb.Mapping
{
    /// <summary>Descirbes a builder for registering mapping repositories with <see cref="IEntityContextFactory"/>.</summary>
    public interface IMappingBuilder
    {
        /// <summary>Registers both fluent and attrbiute mappings from an assembly.</summary>
        void FromAssemblyOf<T>();

        /// <summary>Registers both fluent and attrbiute mappings from an assembly.</summary>
        void FromAssembly(Assembly assembly);

        /// <summary>Adds a given mapping.</summary>
        /// <typeparam name="TMappingRepository">Type of the mapping repository.</typeparam>
        /// <param name="mappingAssembly">Source assembly.</param>
        /// <param name="mappingProvider">Mappings provider.</param>
        void AddMapping<TMappingRepository>(Assembly mappingAssembly, TMappingRepository mappingProvider) where TMappingRepository : IMappingProviderSource;
    }
}