using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace RomanticWeb.Entities.Proxies
{
    internal static class DynamicExtensions
    {
        private const string AssemblyNameString = "RomanticWeb.Proxies";
        private static readonly AssemblyName AssemblyName = new AssemblyName(AssemblyNameString);
#if NETSTANDARD16
        private static readonly AssemblyBuilder AssemblyBuilder = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
#else
        private static readonly AssemblyBuilder AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
#endif
        private static readonly ModuleBuilder ModuleBuilder = AssemblyBuilder.DefineDynamicModule("RomanticWeb.Proxies.dll");
        private static readonly MethodInfo InvokeGetMethodInfo = typeof(DynamicExtensions).GetTypeInfo().GetMethod("InvokeGet");
        private static readonly MethodInfo InvokeSetMethodInfo = typeof(DynamicExtensions).GetTypeInfo().GetMethod("InvokeSet");
        private static readonly IDictionary<int, CallSite<Func<CallSite, object, object>>> GetCallSites = new ConcurrentDictionary<int, CallSite<Func<CallSite, object, object>>>();
        private static readonly IDictionary<int, CallSite<Action<CallSite, object, object>>> SetCallSites = new ConcurrentDictionary<int, CallSite<Action<CallSite, object, object>>>();
        private static readonly CSharpArgumentInfo[] InvokeGetArgs = { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
        private static readonly CSharpArgumentInfo[] InvokeSetArgs =
            {
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
            };

        /// <summary>Dynamically casts a given <paramref name="instance" /> to an interface of type <typeparamref name="T" />.</summary>
        /// <typeparam name="T">Type of the interface to proxy.</typeparam>
        /// <param name="instance">Entity to be casted.</param>
        /// <returns>Entity of type <typeparamref name="T" />.</returns>
        public static T ActLike<T>(this object instance) where T : IEntity
        {
            return (T)instance.ActLike(new[] { typeof(T) });
        }

        /// <summary>Dynamically casts a given <paramref name="instance" /> to implement given <paramref name="types" />.</summary>
        /// <param name="instance">Entity to be casted.</param>
        /// <param name="types">Types to implement.</param>
        /// <returns>Entity of given <paramref name="types" />.</returns>
        public static dynamic ActLike(this object instance, Type[] types)
        {
            var proxy = instance as IProxy;
            if (proxy != null)
            {
                instance = proxy;
            }

            types = new[] { typeof(IEntity) }.Union(types).ToArray();
            string name = String.Format("ProxyOf_{0}_Hash", types.Aggregate(0, (current, item) => current ^ item.GetHashCode()));
            var type = AssemblyBuilder.GetType(name) ?? CompileResultType(name, types.Union(types.SelectMany(item => item.GetTypeInfo().GetInterfaces())).ToArray());
            var result = type.GetTypeInfo().GetConstructors().First().Invoke(new[] { instance });
            return result;
        }

        /// <summary>Gets a value of a given property of a dynamic <paramref name="target" /> object.</summary>
        /// <param name="target">Target object .</param>
        /// <param name="propertyName">Property name to be get.</param>
        /// <returns>Value of the property.</returns>
        public static object InvokeGet(dynamic target, string propertyName)
        {
            int binderHash = propertyName.GetHashCode() ^ target.GetType().GetHashCode();
            CallSite<Func<CallSite, object, object>> callSite;
            if (!GetCallSites.TryGetValue(binderHash, out callSite))
            {
                var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(CSharpBinderFlags.None, propertyName, target.GetType(), InvokeGetArgs);
                GetCallSites[binderHash] = callSite = CallSite<Func<CallSite, object, object>>.Create(binder);
            }

            return callSite.Target(callSite, target);
        }

        /// <summary>Sets a value of a given property of a dynamic <paramref name="target" /> object.</summary>
        /// <param name="target">Target object .</param>
        /// <param name="propertyName">Property name to be set.</param>
        /// <param name="value">Value to be set.</param>
        /// <returns>Value of the property.</returns>
        public static void InvokeSet(dynamic target, string propertyName, object value)
        {
            int binderHash = propertyName.GetHashCode() ^ target.GetType().GetHashCode();
            CallSite<Action<CallSite, object, object>> callSite;
            if (!SetCallSites.TryGetValue(binderHash, out callSite))
            {
                var binder = Microsoft.CSharp.RuntimeBinder.Binder.SetMember(CSharpBinderFlags.None, propertyName, target.GetType(), InvokeSetArgs);
                SetCallSites[binderHash] = callSite = CallSite<Action<CallSite, object, object>>.Create(binder);
            }

            callSite.Target(callSite, target, value);
        }

        private static Type CompileResultType(string name, Type[] types)
        {
            TypeBuilder typeBuilder = GetTypeBuilder(name, types);
            FieldBuilder wrappedObjectFieldBuilder = typeBuilder.DefineField("_wrappedObject", typeof(object), FieldAttributes.Private);
            typeBuilder.CreateConstructor(wrappedObjectFieldBuilder);
            typeBuilder.CreateProperty(typeof(IProxy).GetTypeInfo().GetProperty("WrappedObject"), wrappedObjectFieldBuilder, true);
            foreach (var property in from type in types from property in type.GetTypeInfo().GetProperties() where property.CanRead select property)
            {
                typeBuilder.CreateProperty(property, wrappedObjectFieldBuilder);
            }

            Type objectType = typeBuilder.CreateTypeInfo().AsType();
            return objectType;
        }

        private static void CreateConstructor(this TypeBuilder typeBuilder, FieldBuilder wrappedObjectFieldBuilder)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                new[] { typeof(object) });
            var constructorIl = constructorBuilder.GetILGenerator();
            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Call, typeof(ProxyBase).GetTypeInfo().GetConstructor(Type.EmptyTypes));
            constructorIl.Emit(OpCodes.Nop);
            constructorIl.Emit(OpCodes.Nop);
            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Ldarg_1);
            constructorIl.Emit(OpCodes.Stfld, wrappedObjectFieldBuilder);
            constructorIl.Emit(OpCodes.Ret);
        }

        private static void CreateProperty(this TypeBuilder typeBuilder, PropertyInfo property, FieldBuilder wrappedObjectFieldBuilder, bool isWrappedObjectProperty = false)
        {
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, null);
            typeBuilder.CreatePropertyGetter(property, propertyBuilder, wrappedObjectFieldBuilder, isWrappedObjectProperty);
            if (property.CanWrite)
            {
                typeBuilder.CreatePropertySetter(property, propertyBuilder, wrappedObjectFieldBuilder);
            }
        }

        private static void CreatePropertyGetter(
            this TypeBuilder typeBuilder,
            PropertyInfo property,
            PropertyBuilder propertyBuilder,
            FieldBuilder wrappedObjectFieldBuilder,
            bool isWrappedObjectProperty = false)
        {
            MethodBuilder getterBuilder = typeBuilder.DefineMethod(
                "get_" + property.Name,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.FamANDAssem | MethodAttributes.Family,
                property.PropertyType,
                Type.EmptyTypes);
            ILGenerator getIl = getterBuilder.GetILGenerator();

            if (!isWrappedObjectProperty)
            {
                getIl.Emit(OpCodes.Nop);
                getIl.Emit(OpCodes.Ldarg_0);
                getIl.Emit(OpCodes.Ldfld, wrappedObjectFieldBuilder);
                getIl.Emit(OpCodes.Ldstr, property.Name);
                getIl.Emit(OpCodes.Call, InvokeGetMethodInfo);
                getIl.Emit(property.PropertyType.GetTypeInfo().IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, property.PropertyType);
                getIl.Emit(OpCodes.Ret);
            }
            else
            {
                getIl.Emit(OpCodes.Nop);
                getIl.Emit(OpCodes.Ldarg_0);
                getIl.Emit(OpCodes.Ldfld, wrappedObjectFieldBuilder);
                getIl.Emit(OpCodes.Ret);
            }

            propertyBuilder.SetGetMethod(getterBuilder);
            typeBuilder.DefineMethodOverride(getterBuilder, property.GetGetMethod());
        }

        private static void CreatePropertySetter(this TypeBuilder typeBuilder, PropertyInfo property, PropertyBuilder propertyBuilder, FieldBuilder wrappedObjectFieldBuilder)
        {
            MethodBuilder setterBuilder = typeBuilder.DefineMethod(
                "set_" + property.Name,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.FamANDAssem | MethodAttributes.Family,
                null,
                new[] { property.PropertyType });

            ILGenerator setIl = setterBuilder.GetILGenerator();

            setIl.Emit(OpCodes.Nop);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldfld, wrappedObjectFieldBuilder);
            setIl.Emit(OpCodes.Ldstr, property.Name);
            setIl.Emit(OpCodes.Ldarg_1);
            if (property.PropertyType.GetTypeInfo().IsValueType)
            {
                setIl.Emit(OpCodes.Box, property.PropertyType);
            }

            setIl.Emit(OpCodes.Call, InvokeSetMethodInfo);
            setIl.Emit(OpCodes.Nop);
            setIl.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setterBuilder);
            typeBuilder.DefineMethodOverride(setterBuilder, property.GetSetMethod());
        }

        private static TypeBuilder GetTypeBuilder(string name, Type[] types)
        {
            TypeBuilder typeBuilder = ModuleBuilder.DefineType(
                name,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                typeof(ProxyBase),
                new[] { typeof(IProxy) }.Union(types).ToArray());
            return typeBuilder;
        }
    }
}