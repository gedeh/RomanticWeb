#if CASTLE
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Castle.DynamicProxy;
using Microsoft.CSharp.RuntimeBinder;

namespace RomanticWeb.Entities.Proxies
{
    /// <summary>Provides useful extension methods for instances implementing <see cref="IEntity" />.</summary>
    internal static class CastleDynamicExtensions
    {
        private static readonly ProxyGenerationOptions ProxyGeneratorOptions;
        private static readonly ProxyGenerator ProxyGenerator;

        static CastleDynamicExtensions()
        {
            ProxyGeneratorOptions = ProxyGenerationOptions.Default;
            ProxyGeneratorOptions.BaseTypeForInterfaceProxy = typeof(ProxyBase);
            ProxyGenerator = new ProxyGenerator();
        }

        /// <summary>Dynamically casts a given <paramref name="instance" /> to an interface of type <typeparamref name="T" />.</summary>
        /// <typeparam name="T">Type of the interface to proxy.</typeparam>
        /// <param name="instance">Entity to be casted.</param>
        /// <returns>Entity of type <typeparamref name="T" />.</returns>
        public static T ActLike<T>(this object instance) where T : IEntity
        {
            var proxy = instance as IProxyTargetAccessor;
            if (proxy != null)
            {
                instance = proxy.DynProxyGetTarget();
            }

            return (T)ProxyGenerator.CreateInterfaceProxyWithTarget(typeof(IEntity), new[] { typeof(T) }, instance, ProxyGeneratorOptions, ProxyInterceptor.Default);
        }

        /// <summary>Dynamically casts a given <paramref name="instance" /> to implement given <paramref name="types" />.</summary>
        /// <param name="instance">Entity to be casted.</param>
        /// <param name="types">Types to implement.</param>
        /// <returns>Entity of given <paramref name="types" />.</returns>
        public static dynamic ActLike(this object instance, Type[] types)
        {
            var proxy = instance as IProxyTargetAccessor;
            if (proxy != null)
            {
                instance = proxy.DynProxyGetTarget();
            }

            return ProxyGenerator.CreateInterfaceProxyWithTarget(typeof(IEntity), types.Except(new[] { typeof(IEntity) }).ToArray(), instance, ProxyGeneratorOptions, ProxyInterceptor.Default);
        }

        internal static object InvokeGet(dynamic target, string propertyName)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, propertyName, target.GetType(), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            return callsite.Target(callsite, target);
        }

        internal static void InvokeSet(dynamic target, string propertyName, object value)
        {
            var args = new[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                };
            var binder = Binder.SetMember(CSharpBinderFlags.None, propertyName, target.GetType(), args);
            var callsite = CallSite<Action<CallSite, object, object>>.Create(binder);
            callsite.Target(callsite, target, value);
        }

        public class ProxyBase
        {
            public override bool Equals(object obj)
            {
                var proxy = this as IProxyTargetAccessor;
                if (proxy == null)
                {
                    return base.Equals(obj);
                }

                var target = proxy.DynProxyGetTarget();
                return (target == null ? base.Equals(obj) : target.Equals(obj));
            }

            public override int GetHashCode()
            {
                var proxy = this as IProxyTargetAccessor;
                if (proxy == null)
                {
                    return base.GetHashCode();
                }

                var target = proxy.DynProxyGetTarget();
                return (target == null ? base.GetHashCode() : target.GetHashCode());
            }
        }

        private class ProxyInterceptor : IInterceptor
        {
            internal static readonly ProxyInterceptor Default = new ProxyInterceptor();

            /// <inheritdoc />
            public void Intercept(IInvocation invocation)
            {
                if (!invocation.Method.IsSpecialName)
                {
                    throw new NotSupportedException("Invocations of non-property members is forbidden.");
                }

                var target = ((IProxyTargetAccessor)invocation.Proxy).DynProxyGetTarget();
                switch (invocation.Method.Name.Substring(0, 4))
                {
                    case "get_":
                    {
                        var binder = Binder.GetMember(CSharpBinderFlags.None, invocation.Method.Name.Substring(4), target.GetType(), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
                        var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
                        invocation.ReturnValue = callsite.Target(callsite, target);
                        break;
                    }

                    case "set_":
                    {
                        var arguments = new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                        var binder = Binder.SetMember(CSharpBinderFlags.None, invocation.Method.Name.Substring(4), target.GetType(), arguments);
                        var callsite = CallSite<Action<CallSite, object, object>>.Create(binder);
                        callsite.Target(callsite, target, invocation.Arguments[0]);
                        break;
                    }
                }
            }
        }
    }
}
#endif