#if !NETSTANDARD1_6
using System.Configuration;
#endif

namespace RomanticWeb.Configuration
{
    /// <summary>Mapping assembly configuration element.</summary>
#if NETSTANDARD1_6
    public class MappingAssemblyElement
    {
        /// <summary>Gets or sets the assembly name.</summary>
        public string Assembly { get; set; }
    }
#else
    public class MappingAssemblyElement : ConfigurationElement
    {
        private const string AssemlyAttributeName = "assembly";

        /// <summary>Gets or sets the assembly name.</summary>
        [ConfigurationProperty(AssemlyAttributeName)]
        public string Assembly
        {
            get { return (string)this[AssemlyAttributeName]; }
            set { this[AssemlyAttributeName] = value; }
        }
    }
#endif
}
