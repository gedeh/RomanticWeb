using System;
#if NETSTANDARD16
using System.Collections;
#endif
using System.Collections.Generic;
#if NETSTANDARD16
using Microsoft.Extensions.Configuration;
#else
using System.Configuration;
using System.Xml;
#endif

namespace RomanticWeb.DotNetRDF.Configuration
{
    /// <summary>Configuration elements for dotNetRDF triple stores.</summary>
    public class StoresCollection :
#if NETSTANDARD16
        IEnumerable<StoreElement>
#else
        ConfigurationElementCollection, IEnumerable<StoreElement>
#endif
    {
#if NETSTANDARD16
        private static readonly IDictionary<string, Func<StoresCollection, IConfigurationSection, StoreElement>> StoreElementFactories;
        private readonly ICollection<StoreElement> _stores;
#else
        private static readonly IDictionary<string, Func<StoresCollection, StoreElement>> StoreElementFactories;
#endif
        private readonly StoresConfigurationSection _parent;

        static StoresCollection()
        {
#if NETSTANDARD16
            StoreElementFactories = new Dictionary<string, Func<StoresCollection, IConfigurationSection, StoreElement>>();
            StoreElementFactories["inMemory"] = (self, section) => new InMemoryStoreElement(section);
            StoreElementFactories["file"] = (self, section) => new FileStoreElement(section);
            StoreElementFactories["persistent"] = (self, section) => new PersistentStoreElement(section);
            StoreElementFactories["external"] = (self, section) => new ExternallyConfiguredStoreElement(section, self._parent);
#else
            StoreElementFactories = new Dictionary<string, Func<StoresCollection, StoreElement>>();
            StoreElementFactories["inMemory"] = self => new InMemoryStoreElement();
            StoreElementFactories["file"] = self => new FileStoreElement();
            StoreElementFactories["persistent"] = self => new PersistentStoreElement();
            StoreElementFactories["external"] = self => new ExternallyConfiguredStoreElement(self._parent);
#endif
        }

        /// <summary>Initializes a new instance of the <see cref="StoresCollection"/> class.</summary>
        /// <param name="parent">The parent configuraion section.</param>
        public StoresCollection(StoresConfigurationSection parent)
        {
            _parent = parent;
#if NETSTANDARD16
            _stores = new List<StoreElement>();
#endif
        }

        /// <inheritdoc/>
        IEnumerator<StoreElement> IEnumerable<StoreElement>.GetEnumerator()
        {
#if NETSTANDARD16
            return _stores.GetEnumerator();
#else
            foreach (var storeElement in this)
            {
                yield return (StoreElement)storeElement;
            }
#endif
        }

#if NETSTANDARD16
        /// <inheritdoc/>
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<StoreElement>)this).GetEnumerator();
        }

        internal void Initialize(IConfigurationSection section)
        {
            foreach (var subSection in section.GetChildren())
            {
                Func<StoresCollection, IConfigurationSection, StoreElement> factory;
                if (StoreElementFactories.TryGetValue(subSection.Key, out factory))
                {
                    foreach (var store in subSection.GetChildren())
                    {
                        var storeInstance = factory(this, store);
                        storeInstance.Name = subSection.Key;
                        _stores.Add(storeInstance);
                    }
                }
            }
        }
#else
        internal void Deserialize(XmlReader reader)
        {
            DeserializeElement(reader, false);
        }

        /// <summary>Not implemented.</summary>
        protected override ConfigurationElement CreateNewElement()
        {
            throw new InvalidOperationException();
        }

        /// <summary>Gets <see cref="StoreElement.Name"/>.</summary>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((StoreElement)element).Name;
        }

        /// <summary>Tries to deserialize a store element node.</summary>
        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            if (StoreElementFactories.ContainsKey(elementName))
            {
                StoreElement storeElement = StoreElementFactories[elementName].Invoke(this);
                storeElement.DeserializeElementForConfig(reader, false);
                BaseAdd(storeElement);
                return true;
            }

            return base.OnDeserializeUnrecognizedElement(elementName, reader);
        }
#endif
    }
}