using System.Dynamic;

namespace RomanticWeb.Entities.Proxies
{
    /// <summary>Base class of the proxy object.</summary>
    public class ProxyBase
    {
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var proxy = this as IProxy;
            if (proxy == null)
            {
                return base.Equals(obj);
            }

            var target = proxy.WrappedObject;
            return (target == null ? base.Equals(obj) : target.Equals(obj));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var proxy = this as IProxy;
            if (proxy == null)
            {
                return base.GetHashCode();
            }

            var target = proxy.WrappedObject;
            return (target == null ? base.GetHashCode() : target.GetHashCode());
        }
    }
}
