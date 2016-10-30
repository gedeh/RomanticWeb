#if NETSTANDARD16
using System;
using Microsoft.Extensions.Configuration;
#else
using System.Configuration;
using System.Xml;
#endif
using VDS.RDF;

namespace RomanticWeb.DotNetRDF.Configuration
{
    /// <summary>Configuration of a dotNetRDF triple store.</summary>
#if NETSTANDARD16
    public abstract class StoreElement : ITripleStoreFactory
    {
        /// <summary>Initializes a new instance of the <see cref="StoreElement" /> class.</summary>
        /// <param name="configurationSection">Source configuration section.</param>
        protected StoreElement(IConfigurationSection configurationSection)
        {
            if (configurationSection == null)
            {
                throw new ArgumentNullException("configurationSection");
            }

            ConfigurationSection = configurationSection;
            Name = configurationSection.Key;
        }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Creates the triple store.</summary>
        public abstract ITripleStore CreateTripleStore();

        protected IConfigurationSection ConfigurationSection { get; private set; }
    }
#else
    public abstract class StoreElement : ConfigurationElement, ITripleStoreFactory
    {
        private const string NameAttributeName = "name";

        /// <summary>Gets or sets the name.</summary>
        [ConfigurationProperty(NameAttributeName, IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this[NameAttributeName]; }
            set { this[NameAttributeName] = value; }
        }

        /// <summary>Creates the triple store.</summary>
        public abstract ITripleStore CreateTripleStore();

        internal void DeserializeElementForConfig(XmlReader reader, bool serializeCollectionKey)
        {
            DeserializeElement(reader, serializeCollectionKey);
        }
    }
#endif
}