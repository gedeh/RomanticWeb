using System;
using System.Reflection;
using RomanticWeb.Mapping.Fluent;

namespace RomanticWeb.Mapping
{
    internal static class TypeExtensions
    {
        internal static bool IsConstructableEntityMap(this Type mappingType)
        {
            var typeInfo = mappingType.GetTypeInfo();
            var test1 = typeof(EntityMap);
            var test2 = mappingType;
            var test3 = typeof(EntityMap).IsAssignableFrom(mappingType);
            return (typeof(EntityMap).IsAssignableFrom(mappingType)) &&
                (!typeInfo.IsAbstract) &&
                (typeInfo.GetConstructor(Type.EmptyTypes) != null) &&
                (!typeInfo.IsGenericTypeDefinition);
        }
    }
}
