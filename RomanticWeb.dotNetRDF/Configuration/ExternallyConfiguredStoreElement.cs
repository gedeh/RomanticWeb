using System;
using System.Configuration;
#if NETSTANDARD1_6
using Microsoft.Extensions.Configuration;
#endif
using RomanticWeb.Configuration;
using VDS.RDF;
using VDS.RDF.Configuration;

namespace RomanticWeb.DotNetRDF.Configuration
{
    /// <summary>Configuration element for a triple store configured in a dotNetRDF configuration file.</summary>
    public class ExternallyConfiguredStoreElement : StoreElement
    {
#if NETSTANDARD1_6
        private Uri _objectUri = null;
#else
        private const string BnodeIdAttributeName = "blankNode";
        private const string UriAttributeName = "uri";
        private const string ConfigurationFileAttributeName = "dnrConfigurationfile";
#endif

        private readonly StoresConfigurationSection _stores;
        private IConfigurationLoader _configurationLoader;

#if NETSTANDARD1_6
        /// <summary>Initializes a new instance of the <see cref="ExternallyConfiguredStoreElement"/> class.</summary>
        /// <param name="configurationSection">Source configuration section.</param>
        /// <param name="stores">Stores configuration section.</param>
        public ExternallyConfiguredStoreElement(IConfigurationSection configurationSection, StoresConfigurationSection stores) : base(configurationSection)
        {
            _stores = stores;
        }
#else
        /// <summary>Initializes a new instance of the <see cref="ExternallyConfiguredStoreElement"/> class.</summary>
        /// <param name="stores">Stores configuration section.</param>
        public ExternallyConfiguredStoreElement(StoresConfigurationSection stores)
        {
            _stores = stores;
        }
#endif

        /// <summary>Gets or sets the blank node identifier of configured store.</summary>
#if NETSTANDARD1_6
        public string BlankNodeIdentifier { get; set; }
#else
        [ConfigurationProperty(BnodeIdAttributeName)]
        public string BlankNodeIdentifier
        {
            get { return (string)this[BnodeIdAttributeName]; }
            set { this[BnodeIdAttributeName] = value; }
        }
#endif

        /// <summary>Gets or sets the object URI of configured store.</summary>
#if NETSTANDARD1_6
        public Uri ObjectUri
        {
            get
            {
                return _objectUri;
            }

            set
            {
                new UriValidator().Validate(value);
                _objectUri = value;
            }
        }
#else
        [ConfigurationProperty(UriAttributeName)]
        [UriValidator]
        public Uri ObjectUri
        {
            get
            {
                return (Uri)this[UriAttributeName];
            }

            set
            {
                this[UriAttributeName] = value;
            }
        }
#endif

        /// <summary>Gets or sets the name of the configuration as declared in the configuration section.</summary>
#if NETSTANDARD1_6
        public string ConfigurationName { get; set; }
#else
        [ConfigurationProperty(ConfigurationFileAttributeName, IsRequired = true)]
        public string ConfigurationName
        {
            get { return (string)this[ConfigurationFileAttributeName]; }
            set { this[ConfigurationFileAttributeName] = value; }
        }
#endif

        internal IConfigurationLoader ConfigurationLoader
        {
            get
            {
                return _configurationLoader ?? (_configurationLoader = _stores.OpenConfiguration(ConfigurationName));
            }

            set
            {
                _configurationLoader = value;
            }
        }

        /// <summary>Creates the triple store by loading it from the relevant configuration file.</summary>
        public override ITripleStore CreateTripleStore()
        {
            bool isUriSet = ObjectUri != null;
            bool isBnodeSet = !string.IsNullOrWhiteSpace(BlankNodeIdentifier);

            if (isUriSet && isBnodeSet)
            {
                throw new ConfigurationErrorsException("Cannot set both blank node and uri");
            }

            if (!(isBnodeSet || isUriSet))
            {
                throw new ConfigurationErrorsException("Either blank node or uri must be set");
            }

            return (isBnodeSet ? ConfigurationLoader.LoadObject<ITripleStore>(BlankNodeIdentifier) : ConfigurationLoader.LoadObject<ITripleStore>(ObjectUri));
        }
    }
}