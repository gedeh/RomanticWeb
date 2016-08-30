using System;
using System.Reflection.Emit;

namespace RomanticWeb.Dynamic
{
    internal static class EmitExtensions
    {
        public static Type GetOrEmitType(this ModuleBuilder moduleBuilder, string typeName, Func<ModuleBuilder, TypeBuilder> emitType)
        {
            return moduleBuilder.GetType(typeName) != null ?
                moduleBuilder.GetType(typeName, true) :
                emitType(moduleBuilder).CreateTypeInfo().AsType();
        }
    }
}