using System;
#if !NETSTANDARD1_6
using System.Configuration;
#endif

namespace RomanticWeb.Configuration
{
    /// <summary>An ontology configuration element.</summary>
#if NETSTANDARD1_6
    public class OntologyElement
    {
        private Uri _uri;

        /// <summary>Gets or sets the ontology prefix.</summary>
        public string Prefix { get; set; }

        /// <summary>Gets or sets the ontology URI.</summary>
        public Uri Uri
        {
            get { return _uri; }
            set { UriValidator.Default.Validate(_uri = value); }
        }
    }
#else
    public class OntologyElement : ConfigurationElement
    {
        private const string PrefixAttributeName = "prefix";
        private const string UriAttributeName = "uri";

        /// <summary>Gets or sets the ontology prefix.</summary>
        [ConfigurationProperty(PrefixAttributeName, IsRequired = true, IsKey = true)]
        public string Prefix
        {
            get { return (string)this[PrefixAttributeName]; }
            set { this[PrefixAttributeName] = value; }
        }

        /// <summary>Gets or sets the ontology URI.</summary>
        [ConfigurationProperty(UriAttributeName, IsRequired = true, IsKey = true)]
        [UriValidator]
        public Uri Uri
        {
            get { return (Uri)this[UriAttributeName]; }
            set { this[UriAttributeName] = value; }
        }
    }
#endif
}
