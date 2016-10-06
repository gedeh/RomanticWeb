using System;
using System.Collections.Generic;
#if !NETSTANDARD16
using System.Configuration;
using System.Linq;
#endif

namespace RomanticWeb.DotNetRDF.Configuration.StorageProviders
{
    internal class CustomProviderElement : StorageProviderElement
    {
#if NETSTANDARD16
        public string TypeName { get; set; }

        public IDictionary<string, string> ConstructorParametersElement { get; set; }
#else
        private const string TypeAttributeName = "type";
        private const string ConstructorParametersElementName = "parameters";

        [ConfigurationProperty(TypeAttributeName, IsRequired = true)]
        [CallbackValidator(CallbackMethodName = "ValidateType", Type = typeof(Validators))]
        public string TypeName
        {
            get { return (string)this[TypeAttributeName]; }
            set { this[TypeAttributeName] = value; }
        }

        [ConfigurationProperty(ConstructorParametersElementName)]
        public KeyValueConfigurationCollection ConstructorParametersElement
        {
            get { return (KeyValueConfigurationCollection)this[ConstructorParametersElementName]; }
            set { this[ConstructorParametersElementName] = value; }
        }
#endif
        protected override IDictionary<string, string> ConstructorParameters
        {
            get
            {
#if NETSTANDARD16
                return ConstructorParametersElement;
#else
                return (from KeyValueConfigurationElement element in ConstructorParametersElement select element).ToDictionary(e => e.Key, e => e.Value);
#endif
            }
        }

        protected override Type ProviderType { get { return Type.GetType(TypeName); } }
    }
}