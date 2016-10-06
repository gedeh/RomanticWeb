#if !NETSTANDARD16
using System.Configuration;
#endif

namespace RomanticWeb.DotNetRDF.Configuration
{
    /// <summary>Configuration for a dotNetRDF configuration file.</summary>
#if NETSTANDARD16
    public class ConfigurationFileElement
    {
        /// <summary>Initializes a new instance of the <see cref="ConfigurationFileElement" /> class.</summary>
        public ConfigurationFileElement()
        {
            AutoConfigure = true;
        }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the path.</summary>
        public string Path { get; set; }

        /// <summary>Gets or sets a value indicating whether configuration should automatically configured.</summary>
        public bool AutoConfigure { get; set; }
    }
#else
    public class ConfigurationFileElement : ConfigurationElement
    {
        private const string NameAttributeName = "name";
        private const string PathAttributeName = "path";
        private const string AutoConfigureAttributeName = "autoConfigure";

        /// <summary>Gets or sets the name.</summary>
        [ConfigurationProperty(NameAttributeName)]
        public string Name
        {
            get { return (string)this[NameAttributeName]; }
            set { this[NameAttributeName] = value; }
        }

        /// <summary>Gets or sets the path.</summary>
        [ConfigurationProperty(PathAttributeName)]
        public string Path
        {
            get { return (string)this[PathAttributeName]; }
            set { this[PathAttributeName] = value; }
        }

        /// <summary>Gets or sets a value indicating whether configuration should automatically configured.</summary>
        [ConfigurationProperty(AutoConfigureAttributeName, DefaultValue = true)]
        public bool AutoConfigure
        {
            get { return (bool)this[AutoConfigureAttributeName]; }
            set { this[AutoConfigureAttributeName] = value; }
        }
    }
#endif
}