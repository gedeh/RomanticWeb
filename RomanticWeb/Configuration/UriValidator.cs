using System;
#if !NETSTANDARD16
using System.Configuration;
#endif

namespace RomanticWeb.Configuration
{
    /// <summary>Provides validation of <see cref="Uri"/> values.</summary>
    public class UriValidator : ConfigurationValidatorBase
    {
#if NETSTANDARD16
        internal static readonly UriValidator Default = new UriValidator();
#endif
        /// <inheritdoc />
        public override bool CanValidate(Type type)
        {
            return type == typeof(Uri);
        }

        /// <inheritdoc />
        public override void Validate(object value)
        {
            var uri = value as Uri;

            if ((uri != null) && (!uri.IsAbsoluteUri))
            {
                throw new ArgumentException("Ontology must be a valid absolute URI");
            }
        }
    }
}