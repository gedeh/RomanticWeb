using System;
using System.Reflection;
using System.Reflection.Emit;

namespace RomanticWeb.Dynamic
{
    internal class EmitHelper
    {
        private const string DynamicMappingModuleName = "DynamicDictionaryMappings";
        private static readonly object Locker = new object();
        private readonly Lazy<AssemblyBuilder> _asmBuilder;
        private readonly Guid _assemblyGuid = Guid.NewGuid();
        private ModuleBuilder _moduleBuilder;

        public EmitHelper()
        {
            _asmBuilder = new Lazy<AssemblyBuilder>(CreateBuilder, true);
        }

        public ModuleBuilder GetDynamicModule()
        {
            var assemblyBuilder = GetBuilder();
            lock (Locker)
            {
                return _moduleBuilder ?? (_moduleBuilder = assemblyBuilder.DefineDynamicModule(DynamicMappingModuleName));
            }
        }

        public AssemblyBuilder GetBuilder()
        {
            return _asmBuilder.Value;
        }

        private AssemblyBuilder CreateBuilder()
        {
            var asmName = new AssemblyName("RomanticWeb.Dynamic_" + _assemblyGuid.ToString("N"));
#if NETSTANDARD16
            return AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
#else
            return AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
#endif
        }
    }
}