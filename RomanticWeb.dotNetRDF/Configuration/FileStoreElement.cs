#if !NETSTANDARD16
using System.Configuration;
#else
using Microsoft.Extensions.Configuration;
#endif
using VDS.RDF;

namespace RomanticWeb.DotNetRDF.Configuration
{
    /// <summary>Configuration element for in-memory triple store connected with a file source.</summary>
#if NETSTANDARD16
    public class FileStoreElement : StoreElement
    {
        /// <summary>Initializes a new instance of the <see cref="FileStoreElement" /> class.</summary>
        /// <param name="configurationSection">Source configuration section.</param>
        public FileStoreElement(IConfigurationSection configurationSection) : base(configurationSection)
        {
            FilePath = configurationSection.GetValue<string>("filePath");
        }

        /// <summary>Gets or sets the dataset file path.</summary>
        public string FilePath { get; set; }

        /// <inheritdoc />
        public override ITripleStore CreateTripleStore()
        {
            return new FileTripleStore(FilePath);
        }
    }
#else
    public class FileStoreElement : StoreElement
    {
        private const string FilePathAttributeName = "filePath";

        /// <summary>Gets or sets the dataset file path.</summary>
        [ConfigurationProperty(FilePathAttributeName, IsRequired = true)]
        public string FilePath
        {
            get { return (string)this[FilePathAttributeName]; }
            set { this[FilePathAttributeName] = value; }
        }

        /// <inheritdoc />
        public override ITripleStore CreateTripleStore()
        {
            return new FileTripleStore(FilePath);
        }
    }
#endif
}