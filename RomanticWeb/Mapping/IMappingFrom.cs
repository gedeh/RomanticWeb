using System.Reflection;

namespace RomanticWeb.Mapping
{
    /// <summary>Describes an abstract mapping source.</summary>
    public interface IMappingFrom
    {
        /// <summary>Instructs to gather mappings from the assembly containing a <typeparamref name="T" />.</summary>
        /// <typeparam name="T">Type that points to the target assembly to scan.</typeparam>
        /// <param name="builder">Target mapping builder.</param>
        void FromAssemblyOf<T>(IMappingBuilder builder);

        /// <summary>Instructs to gather mappings from the given <paramref name="assembly"/>.</summary>
        /// <param name="builder">Target mapping builder.</param>
        /// <param name="assembly">Target assembly to scan.</param>
        void FromAssembly(IMappingBuilder builder, Assembly assembly);
    }
}
