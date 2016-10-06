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
        private static readonly IDictionary<string, Func<StorageProviderElement>> ProviderElementFactories;
        private StorageProviderElement _storageProvider;

        static PersistentStoreElement()
        {
            ProviderElementFactories = new Dictionary<string, Func<StorageProviderElement>>();
            //// TODO: Virtuoso in CORE version required!
#if !NETSTANDARD16
            ProviderElementFactories["virtuosoManager"] = () => new VirtuosoManagerElement();
#endif
            ProviderElementFactories["allegroGraphConnector"] = () => new AllegroGraphConnectorElement();
            ProviderElementFactories["customProvider"] = () => new CustomProviderElement();
        }
#if NETSTANDARD16
        /// <summary>Initializes a new instance of the <see cref="PersistentStoreElement" /> class.</summary>
        /// <param name="configurationSection">Source configuration section.</param>
        public PersistentStoreElement(IConfigurationSection configurationSection) : base(configurationSection)
        {
            Func<StorageProviderElement> factory;
            if (!ProviderElementFactories.TryGetValue(configurationSection.Key, out factory))
            {
                throw new InvalidOperationException(String.Format("Cannot create a persistent store of type '{0}'.", configurationSection.Key));
            }

            StorageProvider = factory();
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