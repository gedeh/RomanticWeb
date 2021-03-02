using System;
using System.IO;
using System.Reflection;
#if NETSTANDARD1_6
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
#endif

namespace RomanticWeb.Configuration
{
    internal static class MappingAssemblyElementExtensions
    {
#if NETSTANDARD1_6
        private static readonly string AssemblyPath;

        static MappingAssemblyElementExtensions()
        {
            AssemblyPath = Path.GetDirectoryName(EnsureAbsolutePath(Assembly.GetEntryAssembly().CodeBase));
        }
#endif
        internal static Assembly Load(this MappingAssemblyElement mappingAssemblyElement)
        {
#if NETSTANDARD1_6
            return StringComparer.OrdinalIgnoreCase.Equals(Assembly.GetEntryAssembly().GetName().Name, mappingAssemblyElement.Assembly) ?
                Assembly.GetEntryAssembly() :
                AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(AssemblyPath, mappingAssemblyElement.Assembly + ".dll"));
#else
            return Assembly.Load(mappingAssemblyElement.Assembly);
#endif
        }

        private static string EnsureAbsolutePath(string path) {
            if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase)) {
                return Path.GetFullPath(new Uri(path).LocalPath).Replace("/", Path.DirectorySeparatorChar.ToString());
            }

            return path;
        }
    }
}
