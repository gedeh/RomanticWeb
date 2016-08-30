namespace RomanticWeb.Entities.Proxies
{
    public class Example : ProxyBase, IProxy, IEntity
    {
        private object _wrappedObject;

        public Example(object wrappedObject)
        {
            _wrappedObject = wrappedObject;
        }

        public object WrappedObject { get { return _wrappedObject; } }

        public EntityId Id { get { return (EntityId)DynamicExtensions.InvokeGet(_wrappedObject, "Id"); } }

        public IEntityContext Context { get { return (IEntityContext)DynamicExtensions.InvokeGet(_wrappedObject, "Context"); } }

        public string Test { set { DynamicExtensions.InvokeSet(_wrappedObject, "Test", value); } }

        public int Test2
        {
            get { return (int)DynamicExtensions.InvokeGet(_wrappedObject, "Test2"); }
            set { DynamicExtensions.InvokeSet(_wrappedObject, "Test2", value); }
        }
    }
}
