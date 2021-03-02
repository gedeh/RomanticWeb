#if NETSTANDARD1_6
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
#else
using System.Configuration;
#endif

namespace RomanticWeb.Configuration
{
    /// <summary>Configuration section for RomanticWeb.</summary>
    public class ConfigurationSectionHandler
#if !NETSTANDARD1_6
        : ConfigurationSection
#endif
    {
        private const string ConfigurationFileName = "appsettings.json";
        private const string DefaultConfigurationName = "romanticWeb";
#if NETSTANDARD1_6
        private static readonly IConfigurationRoot Configuration;
        private static IDictionary<string, ConfigurationSectionHandler> _configurations = new ConcurrentDictionary<string, ConfigurationSectionHandler>();
        private FactoryElement[] _factories = Array.Empty<FactoryElement>();

        static ConfigurationSectionHandler()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(DiscoverConfigurationFile(), true);
            Configuration = configurationBuilder.Build();
        }
#else
        private const string FactoryCollectionElementName = "factories";
#endif

        /// <summary>Gets the configuration from default configutarion section.</summary>
        public static ConfigurationSectionHandler Default { get { return GetConfiguration(DefaultConfigurationName); } }

        /// <summary>Gets or sets the collection of factory configurations.</summary>
#if NETSTANDARD1_6
        public FactoryElement[] Factories
        {
            get { return _factories; }
            set
            {
                foreach (var factory in (_factories = value ?? Array.Empty<FactoryElement>()))
                {
                    factory.Validate();
                }
            }
        }
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

        /// <summary>Gets a configuration by it's <paramref name="name"/>.</summary>
        /// <param name="name">The name of the configuration to obtain.</param>
        /// <returns>Configuration section of a given <paramref name="name"/> or empty configuration.</returns>
        public static ConfigurationSectionHandler GetConfiguration(string name)
        {
#if NETSTANDARD1_6
            ConfigurationSectionHandler result;
            if (!_configurations.TryGetValue(name, out result))
            {
                _configurations[name] = result = new ConfigurationSectionHandler();
                var configurationBinder = new ConfigureFromConfigurationOptions<ConfigurationSectionHandler>(Configuration.GetSection(name));
                try
                {
                    configurationBinder.Configure(result);
                }
                catch (TargetInvocationException error)
                {
                    throw error.InnerException;
                }
            }

            return result;
#else
            return (ConfigurationSectionHandler)ConfigurationManager.GetSection(name) ?? new ConfigurationSectionHandler();
#endif
        }

#if NETSTANDARD1_6
        private static string DiscoverConfigurationFile()
        {
            var path = Directory.GetCurrentDirectory();
            if (!File.Exists(Path.Combine(path, ConfigurationFileName)))
            {
                return (from directory in Directory.GetDirectories(path)
                        from file in Directory.GetFiles(directory, ConfigurationFileName)
                        select file).FirstOrDefault() ?? Path.Combine(path, ConfigurationFileName);
            }

            return Path.Combine(path, ConfigurationFileName);
        }
#endif
    }
}