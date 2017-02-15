#if NETSTANDARD16
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
#if !NETSTANDARD16
        : ConfigurationSection
#endif
    {
        private const string DefaultConfigurationName = "romanticWeb";
#if NETSTANDARD16
        private static readonly IConfigurationRoot Configuration;
        private static IDictionary<string, ConfigurationSectionHandler> _configurations = new ConcurrentDictionary<string, ConfigurationSectionHandler>();
        private FactoryElement[] _factories = Array.Empty<FactoryElement>();

        static ConfigurationSectionHandler()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true);
            Configuration = configurationBuilder.Build();
        }
#else
        private const string FactoryCollectionElementName = "factories";
#endif

        /// <summary>Gets the configuration from default configutarion section.</summary>
        public static ConfigurationSectionHandler Default { get { return GetConfiguration(DefaultConfigurationName); } }

        /// <summary>Gets or sets the collection of factory configurations.</summary>
#if NETSTANDARD16
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
#if NETSTANDARD16
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
    }
}