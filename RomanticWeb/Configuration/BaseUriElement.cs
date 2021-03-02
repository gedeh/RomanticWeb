using System;
#if !NETSTANDARD1_6
using System.Configuration;
#endif
namespace RomanticWeb.Configuration
{
    /// <summary>Configuration element to set base Uri for.</summary>
#if NETSTANDARD1_6
    public class BaseUriElement
    {
        private Uri _default;

        /// <summary>Gets or sets the default base Uri.</summary>
        public Uri Default
        {
            get { return _default; }
            set { UriValidator.Default.Validate(_default = value); }
        }
    }
#else
    public class BaseUriElement : ConfigurationElement
    {
        private const string DefaultUriAttributeName = "default";

        /// <summary>Gets or sets the default base Uri.</summary>
        [ConfigurationProperty(DefaultUriAttributeName)]
        [UriValidator]
        public Uri Default
        {
            get { return (Uri)this[DefaultUriAttributeName]; }
            set { this[DefaultUriAttributeName] = value; }
        }
    }
#endif
}
