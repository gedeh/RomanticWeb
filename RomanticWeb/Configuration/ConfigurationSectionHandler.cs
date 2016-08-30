#if NETSTANDARD16
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
#else
using System.Configuration;
#endif

namespace RomanticWeb.Configuration
{
    /// <summary>Configuration section for RomanticWeb.</summary>
    public class ConfigurationSectionHandler
#if !NETSTANDARD16
        : ConfigurationSection
#endif
    {
#if NETSTANDARD16
        private static readonly IConfigurationRoot Configuration;

        static ConfigurationSectionHandler()
        {
            var configurationBuilder = new ConfigurationBuilder();
            try
            {
                if (File.Exists("appsettings.json"))
                {
                    configurationBuilder.AddJsonFile("appsettings.json");
                }
            }
            catch
            {
            }

            Configuration = configurationBuilder.Build();
        }
#else
        private const string FactoryCollectionElementName = "factories";
#endif

        /// <summary>Gets the configuration from default configutarion section.</summary>
        public static ConfigurationSectionHandler Default
        {
            get
            {
#if NETSTANDARD16
                var configurationBinder = new ConfigureFromConfigurationOptions<ConfigurationSectionHandler>(Configuration.GetSection("romanticWeb"));
                var result = new ConfigurationSectionHandler();
                configurationBinder.Configure(result);
                return result;
#else
                return (ConfigurationSectionHandler)ConfigurationManager.GetSection("romanticWeb") ?? new ConfigurationSectionHandler();
#endif
            }
        }

        /// <summary>Gets or sets the collection of factory configurations.</summary>
#if NETSTANDARD16
        public IEnumerable<FactoryElement> Factories { get; set; }
#else
        /// <summary>Gets or sets the collection of factory configurations.</summary>
        [ConfigurationProperty(FactoryCollectionElementName)]
        [ConfigurationCollection(typeof(FactoriesCollection), AddItemName = "factory")]
        public FactoriesCollection Factories
        {
            get { return (FactoriesCollection)this[FactoryCollectionElementName]; }
            set { this[FactoryCollectionElementName] = value; }
        }
#endif
    }
}