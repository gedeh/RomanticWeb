#if NETSTANDARD1_6
using System;

namespace RomanticWeb.Configuration
{
    public abstract class ConfigurationValidatorBase
    {
        /// <summary>Checks whether the given validator can perform validation.</summary>
        /// <param name="type">Type of the value to validate.</param>
        /// <returns><b>true</b> if the validator is capable of validating a given value; otherwise <b>false</b>.</returns>
        public abstract bool CanValidate(Type type);

        /// <summary>Validates a given value.</summary>
        /// <param name="value">Value to be validated.</param>
        /// <remarks>If the value is invalid, implementeur should throw an exception of type <see cref="ArgumentException" />.</remarks>
        public abstract void Validate(object value);
    }
}
#endif