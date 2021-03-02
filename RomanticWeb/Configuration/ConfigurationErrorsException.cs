#if NETSTANDARD1_6
namespace System.Configuration
{
    /// <summary>Represents a configuration exception.</summary>
    public class ConfigurationErrorsException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="ConfigurationErrorsException" /> class.</summary>
        /// <param name="message">Exception message.</param>
        public ConfigurationErrorsException(string message) : base(message)
        {
        }
    }
}
#endif