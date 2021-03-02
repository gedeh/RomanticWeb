#if NETSTANDARD1_6
using System.IO;
using System.Reflection;
#endif

namespace System
{
    /// <summary>Contains useful extension methods for AppDomain class.</summary>
    internal static class AppDomainExtensions
    {
        /// <summary>Gets a primary path storing assemblies for given application domain.</summary>
        /// <remarks>This method shouldn't reutrn <b>null</b> in any case.</remarks>
        /// <returns>Primary place where assemblies for given application domain are stored.</returns>
        public static string GetPrimaryAssemblyPath()
        {
#if NETSTANDARD1_6
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase.Replace("file:///", String.Empty).Replace("/", Path.DirectorySeparatorChar.ToString()));
#else
            return System.String.IsNullOrWhiteSpace(AppDomain.CurrentDomain.RelativeSearchPath) ?
                AppDomain.CurrentDomain.BaseDirectory :
                AppDomain.CurrentDomain.RelativeSearchPath;
#endif
        }
    }
}
