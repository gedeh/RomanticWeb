#if NETSTANDARD16
using System.IO;
using System.Text.RegularExpressions;

#endif

namespace RomanticWeb.Tests
{
    public class ConfigurationTest
    {
#if NETSTANDARD16
        private static string _binDirectory;

        protected static string BaseDirectory
        {
            get
            {
                if (_binDirectory == null)
                {
                    _binDirectory = Directory.GetCurrentDirectory();
                }

                return _binDirectory;
            }
        }

        protected static string BinDirectory
        {
            get
            {
                var result = BaseDirectory;
                if (!Regex.IsMatch(result, "debug", RegexOptions.IgnoreCase))
                {
                    result = Path.Combine(result, "bin", "debug");
                }

                return result;
            }
        }
#endif
    }
}
