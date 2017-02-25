#if NETSTANDARD16
using System.IO;
#endif

namespace RomanticWeb.Tests
{
    public class ConfigurationTest
    {
#if NETSTANDARD16
        private static string _binDirectory;

        protected static string BinDirectory
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
#endif
    }
}
