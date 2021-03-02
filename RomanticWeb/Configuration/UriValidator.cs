using System;
using System.Configuration;

namespace RomanticWeb.Configuration
{
    /// <summary>Provides validation of <see cref="Uri"/> values.</summary>
    public class UriValidator : ConfigurationValidatorBase
    {
#if NETSTANDARD1_6
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

            if ((uri == null) || (!uri.IsAbsoluteUri))
            {
                throw new ConfigurationErrorsException("Ontology must be a valid absolute URI");
            }
        }
    }
}