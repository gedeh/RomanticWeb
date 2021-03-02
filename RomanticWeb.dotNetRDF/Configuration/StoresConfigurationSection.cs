using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
#if NETSTANDARD16
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
#else
using RomanticWeb.Configuration;
#endif
using VDS.RDF;
using VDS.RDF.Configuration;

namespace RomanticWeb.DotNetRDF.Configuration
{
    /// <summary>Configuration section for triple stores.</summary>
    public class StoresConfigurationSection
#if !NETSTANDARD16
        : ConfigurationSection
#endif
    {
#if NETSTANDARD16
        private static readonly IConfigurationRoot Configuration;
        private static StoresConfigurationSection _default;

        static StoresConfigurationSection()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true);
            Configuration = configurationBuilder.Build();
        }
#else
        private const string StoresCollectionElementName = "stores";
        private const string ConfigurationFilesElementName = "dnrConfigurationFiles";
#endif

        /// <summary>Gets the configuration from default configutarion section.</summary>
        public static StoresConfigurationSection Default
        {
            get
            {
#if NETSTANDARD16
                if (_default == null)
                {
                    _default = new StoresConfigurationSection();
                    var section = Configuration.GetSection("romanticWeb.dotNetRDF");
                    var configurationBinder = new ConfigureFromConfigurationOptions<StoresConfigurationSection>(section);
                    configurationBinder.Configure(_default);
                    (_default.Stores = new StoresCollection(_default)).Initialize(section.GetSection("stores"));
                    return _default;
                }

                return _default;
#else
                return (StoresConfigurationSection)ConfigurationManager.GetSection("romanticWeb.dotNetRDF")
                       ?? new StoresConfigurationSection();
#endif
            }
        }

        /// <summary>Gets or sets the stores.</summary>
        public StoresCollection Stores { get; private set; }

        /// <summary>Gets or sets the configuration files.</summary>
#if NETSTANDARD16
        public ConfigurationFileElement[] ConfigurationFiles { get; set; }
#else
        [ConfigurationProperty(ConfigurationFilesElementName)]
        [ConfigurationCollection(typeof(ConfigurationFilesCollection))]
        public ConfigurationFilesCollection ConfigurationFiles
        {
            get { return (ConfigurationFilesCollection)this[ConfigurationFilesElementName]; }
            set { this[ConfigurationFilesElementName] = value; }
        }
#endif

        /// <summary>Creates a store defined in configuration.</summary>
        public ITripleStore CreateStore(string name)
        {
            return Stores.Single(store => store.Name == name).CreateTripleStore();
        }

        internal IConfigurationLoader OpenConfiguration(string name)
        {
            var configurationFile = ConfigurationFiles.Cast<ConfigurationFileElement>().FirstOrDefault(c => c.Name == name);

            if (configurationFile != null)
            {
#if NETSTANDARD16
                var uri = new Uri(configurationFile.Path, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                {
                    uri = new Uri(Path.Combine(Directory.GetCurrentDirectory(), configurationFile.Path));
                }

                return new ConfigurationLoader(uri, configurationFile.AutoConfigure);
#else
                return new ConfigurationLoader(configurationFile.Path, configurationFile.AutoConfigure);
#endif
            }

            throw new ConfigurationErrorsException(string.Format("Configuration '{0}' wasn't found", name));
        }

#if !NETSTANDARD16
        /// <summary>Tries to deserialize stores collection</summary>
        protected override bool OnDeserializeUnrecognizedElement(string elementName, System.Xml.XmlReader reader)
        {
            if (elementName == StoresCollectionElementName)
            {
                var storesCollection = new StoresCollection(this);
                storesCollection.Deserialize(reader);
                Stores = storesCollection;
                return true;
            }

            return base.OnDeserializeUnrecognizedElement(elementName, reader);
        }
#endif
    }
}