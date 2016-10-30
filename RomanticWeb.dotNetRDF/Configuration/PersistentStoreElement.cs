using System;
using System.Collections.Generic;
using System.Configuration;
#if NETSTANDARD16
using Microsoft.Extensions.Configuration;
#else
using System.Xml;
#endif
using RomanticWeb.DotNetRDF.Configuration.StorageProviders;
using VDS.RDF;

namespace RomanticWeb.DotNetRDF.Configuration
{
    /// <summary>Configuration of a third-party triple store.</summary>
    public class PersistentStoreElement : StoreElement
    {
#if NETSTANDARD16
        private static readonly IDictionary<string, Func<IConfigurationSection, StorageProviderElement>> ProviderElementFactories;
#else
        private static readonly IDictionary<string, Func<StorageProviderElement>> ProviderElementFactories;
#endif
        private StorageProviderElement _storageProvider;

        static PersistentStoreElement()
        {
#if NETSTANDARD16
            ProviderElementFactories = new Dictionary<string, Func<IConfigurationSection, StorageProviderElement>>();
            //// TODO: Virtuoso in CORE version required!
            ProviderElementFactories["allegroGraphConnector"] = section => new AllegroGraphConnectorElement(section);
            ProviderElementFactories["customProvider"] = section => new CustomProviderElement(section);
#else
            ProviderElementFactories = new Dictionary<string, Func<StorageProviderElement>>();
            ProviderElementFactories["virtuosoManager"] = () => new VirtuosoManagerElement();
            ProviderElementFactories["allegroGraphConnector"] = () => new AllegroGraphConnectorElement();
            ProviderElementFactories["customProvider"] = () => new CustomProviderElement();
#endif
        }

#if NETSTANDARD16
        /// <summary>Initializes a new instance of the <see cref="PersistentStoreElement" /> class.</summary>
        /// <param name="configurationSection">Source configuration section.</param>
        public PersistentStoreElement(IConfigurationSection configurationSection) : base(configurationSection)
        {
            Func<IConfigurationSection, StorageProviderElement> factory;
            if (!ProviderElementFactories.TryGetValue(configurationSection.GetValue<string>("providerName"), out factory))
            {
                throw new InvalidOperationException(String.Format("Cannot create a persistent store of type '{0}'.", configurationSection.Key));
            }

            StorageProvider = factory(configurationSection);
        }
#endif

        private StorageProviderElement StorageProvider
        {
            get
            {
                return _storageProvider;
            }

            set
            {
                if (_storageProvider != null)
                {
                    throw new ConfigurationErrorsException("Cannot set two storage providers");
                }

                _storageProvider = value;
            }
        }

        /// <summary>Creates a <see cref="PersistentTripleStore" />.</summary>
        public override ITripleStore CreateTripleStore()
        {
            return new PersistentTripleStore(StorageProvider.CreateStorageProvider());
        }

#if !NETSTANDARD16
        /// <summary>Tries to deserialize known elements representing third-party triple store connector, like Virtuoso, AllegroGraph and others.</summary>
        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            if (ProviderElementFactories.ContainsKey(elementName))
            {
                StorageProviderElement providerElement = ProviderElementFactories[elementName].Invoke();
                providerElement.DeserializeElementForConfig(reader, false);
                StorageProvider = providerElement;
                return true;
            }

            return base.OnDeserializeUnrecognizedElement(elementName, reader);
        }
#endif
    }
}