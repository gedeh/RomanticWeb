using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
#if NETSTANDARD1_6
using Microsoft.Extensions.Configuration;
#else
using System.Xml;
#endif
using VDS.RDF.Storage;

namespace RomanticWeb.DotNetRDF.Configuration.StorageProviders
{
    internal abstract class StorageProviderElement
#if !NETSTANDARD1_6
        : ConfigurationElement
#endif
    {
#if NETSTANDARD1_6
        /// <summary>Initializes a new instance of the <see cref="StorageProviderElement" /> class.</summary>
        protected StorageProviderElement(IConfigurationSection configurationSection)
        {
            ConstructorParameters = new Dictionary<string, string>();
            InitializeConstructorParameters(configurationSection);
        }
#else
        /// <summary>Initializes a new instance of the <see cref="StorageProviderElement" /> class.</summary>
        protected StorageProviderElement()
        {
            ConstructorParameters = new Dictionary<string, string>();
        }
#endif

        protected virtual IDictionary<string, string> ConstructorParameters { get; private set; }

        protected abstract Type ProviderType { get; }

        protected virtual IEnumerable<string> ValidAttributes { get { yield break; } }

        public IStorageProvider CreateStorageProvider()
        {
            var ctorArguments = GetConstructorArguments();
            var ctor = (from constructor in ProviderType.GetConstructors()
                        let parameters = constructor.GetParameters()
                        where (parameters.Length == ctorArguments.Length) &&
                            (parameters.All(parameter => parameter.ParameterType.IsInstanceOfType(ctorArguments[parameter.Position])))
                        select constructor).First();
            return (IStorageProvider)ctor.Invoke(ctorArguments);
        }

#if !NETSTANDARD1_6
        internal void DeserializeElementForConfig(XmlReader reader, bool serializeCollectionKey)
        {
            DeserializeElement(reader, serializeCollectionKey);
        }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            if (ValidAttributes.Contains(name))
            {
                HandleAttribute(name, value);
                return true;
            }

            return base.OnDeserializeUnrecognizedAttribute(name, value);
        }

        protected virtual void HandleAttribute(string name, string value)
        {
            ConstructorParameters.Add(name, value);
        }
#endif
        private ConstructorInfo GetBestMatchingConstructor()
        {
            var constructors = (from ctor in ProviderType.GetConstructors()
                                let parameters = ctor.GetParameters()
                                where parameters.Length == ConstructorParameters.Count
                                where parameters.All(p => ConstructorParameters.ContainsKey(p.Name))
                                select ctor).ToArray();

            if (!constructors.Any())
            {
                throw new ConfigurationErrorsException(
                    String.Format(
                        "Type {0} doesn't contain a public constructor with {1} parameters",
                        ProviderType,
                        ConstructorParameters.Count));
            }

            if (constructors.Length == 1)
            {
                return constructors.Single();
            }

            throw new ConfigurationErrorsException(String.Format("Multiple constructors matched on type {0}", ProviderType));
        }

        private object[] GetConstructorArguments()
        {
            ConstructorInfo constructor = GetBestMatchingConstructor();
            return (from param in constructor.GetParameters()
                    let element = ConstructorParameters[param.Name]
                    select Convert.ChangeType(element, param.ParameterType)).ToArray();
        }
#if NETSTANDARD1_6
        private void InitializeConstructorParameters(IConfigurationSection configurationSection)
        {
            foreach (var property in ValidAttributes)
            {
                var value = configurationSection.GetValue<string>(property);
                if (value != null)
                {
                    ConstructorParameters[property] = value;
                }
            }
        }
#endif
    }
}