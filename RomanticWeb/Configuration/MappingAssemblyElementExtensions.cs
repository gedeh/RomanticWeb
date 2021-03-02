using System;
using System.IO;
using System.Reflection;
#if NETSTANDARD16
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
#endif

namespace RomanticWeb.Configuration
{
    internal static class MappingAssemblyElementExtensions
    {
#if NETSTANDARD16
        private static readonly string AssemblyPath;

        static MappingAssemblyElementExtensions()
        {
            AssemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase.Replace("/", Path.DirectorySeparatorChar.ToString()));
        }
#endif
        internal static Assembly Load(this MappingAssemblyElement mappingAssemblyElement)
        {
#if NETSTANDARD16
            return StringComparer.OrdinalIgnoreCase.Equals(Assembly.GetEntryAssembly().GetName().Name, mappingAssemblyElement.Assembly) ?
                Assembly.GetEntryAssembly() :
                AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(AssemblyPath, mappingAssemblyElement.Assembly + ".dll"));
#else
            return Assembly.Load(mappingAssemblyElement.Assembly);
#endif
        }
    }
}
