#if !NETSTANDARD16
using System.Configuration;
#else
using Microsoft.Extensions.Configuration;
#endif
using VDS.RDF;

namespace RomanticWeb.DotNetRDF.Configuration
{
    /// <summary>Configuration element for in-memory triple store.</summary>
    public class InMemoryStoreElement : StoreElement
    {
#if NETSTANDARD16
        /// <summary>Initializes a new instance of the <see cref="InMemoryStoreElement" /> class.</summary>
        /// <param name="configurationSection">Source configuration section.</param>
        public InMemoryStoreElement(IConfigurationSection configurationSection) : base(configurationSection)
        {
            var threadSafe = configurationSection.GetValue<bool?>("threadSafe");
            ThreadSafe = (threadSafe.HasValue ? threadSafe.Value : true);
        }
#else
        private const string ThreadSafeAttributeName = "threadSafe";
#endif

        /// <summary>Gets or sets a value indicating whether the store should be thread safe.</summary>
#if NETSTANDARD16
        public bool ThreadSafe { get; set; }
#else
        [ConfigurationProperty(ThreadSafeAttributeName, DefaultValue = true)]
        public bool ThreadSafe
        {
            get { return (bool)this[ThreadSafeAttributeName]; }
            set { this[ThreadSafeAttributeName] = value; }
        }
#endif

        /// <inheritdoc />
        public override ITripleStore CreateTripleStore()
        {
            return ThreadSafe ? new ThreadSafeTripleStore() : new TripleStore();
        }
    }
}