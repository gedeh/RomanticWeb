namespace RomanticWeb.Entities.Proxies
{
    /// <summary>Abstract of the proxy object.</summary>
    public interface IProxy
    {
        /// <summary>Gets the wrapped object.</summary>
        object WrappedObject { get; }
    }
}
