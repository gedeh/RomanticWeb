using System;
using System.Reflection;
using RomanticWeb.Mapping.Fluent;

namespace RomanticWeb.Mapping
{
    internal static class TypeExtensions
    {
        internal static bool IsConstructableEntityMap(this Type mappingType)
        {
            var hasConstructor = new Lazy<bool>(() => HasParameterlessConstructor(mappingType));
            return (typeof(EntityMap).IsAssignableFrom(mappingType)) && (!mappingType.GetTypeInfo().IsAbstract) && (hasConstructor.Value) && (!mappingType.GetTypeInfo().IsGenericTypeDefinition);
        }

        private static bool HasParameterlessConstructor(Type mappingType)
        {
            return mappingType.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}
