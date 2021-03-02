﻿using System;
using System.Collections.Generic;
#if NETSTANDARD1_6
using System.Linq;
using Microsoft.Extensions.Configuration;
#else
using System.Configuration;
using System.Linq;
#endif

namespace RomanticWeb.DotNetRDF.Configuration.StorageProviders
{
    internal class CustomProviderElement : StorageProviderElement
    {
#if NETSTANDARD1_6
        private IDictionary<string, string> _parameters = new Dictionary<string, string>();

        internal CustomProviderElement(IConfigurationSection configurationSection) : base(configurationSection)
        {
            TypeName = configurationSection.GetValue<string>("typeName");
            var parameters = configurationSection.GetSection("parameters");
            if (parameters == null)
            {
                return;
            }

            foreach (var parameter in parameters.GetChildren())
            {
                _parameters.Add(parameter.GetValue<string>("key"), parameter.GetValue<string>("value"));
            }
        }

        public string TypeName { get; set; }

        public IDictionary<string, string> Parameters
        {
            get { return _parameters; }
            set { _parameters = value ?? new Dictionary<string, string>(); }
        }
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
#if NETSTANDARD1_6
                return _parameters;
#else
                return (from KeyValueConfigurationElement element in ConstructorParametersElement select element).ToDictionary(e => e.Key, e => e.Value);
#endif
            }
        }

        protected override Type ProviderType { get { return (TypeName != null ? Type.GetType(TypeName) : null); } }
    }
}