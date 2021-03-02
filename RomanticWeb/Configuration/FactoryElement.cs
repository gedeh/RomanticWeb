﻿using System;
using System.Collections.Generic;

#if !NETSTANDARD1_6
using System.Configuration;
using System.Xml;
#endif

namespace RomanticWeb.Configuration
{
    /// <summary>Configuration of a ecntity context factory.</summary>
#if NETSTANDARD1_6
    public class FactoryElement
    {
        private Uri _metaGraphUri;
        private MappingAssemblyElement[] _mappingAssemblies = Array.Empty<MappingAssemblyElement>();
        private OntologyElement[] _ontologies = Array.Empty<OntologyElement>();
        private BaseUriElement _baseUris = new BaseUriElement();

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets a flag indicating whether the internal mechanisms should be thread-safe.</summary>
        public bool ThreadSafe { get; set; }

        /// <summary>Gets or sets a flag indicating whether the changes should be tracked.</summary>
        public bool TrackChanges { get; set; }

        /// <summary>Gets or sets the mapping assemblies.</summary>
        public MappingAssemblyElement[] MappingAssemblies
        {
            get { return _mappingAssemblies; }
            set { _mappingAssemblies = value ?? Array.Empty<MappingAssemblyElement>(); }
        }

        /// <summary>Gets or sets the ontologies configuration element collection.</summary>
        public OntologyElement[] Ontologies
        {
            get { return _ontologies; }
            set { _ontologies = value ?? Array.Empty<OntologyElement>(); }
        }

        /// <summary>Gets or sets the base uri configuration element.</summary>
        public BaseUriElement BaseUris
        {
            get { return _baseUris; }
            set { _baseUris = value ?? new BaseUriElement(); }
        }

        /// <summary>Gets or sets the meta graph URI.</summary>
        public Uri MetaGraphUri
        {
            get { return _metaGraphUri; }
            set { UriValidator.Default.Validate(_metaGraphUri = value); }
        }

        internal void Validate()
        {
            UriValidator.Default.Validate(_metaGraphUri);
        }
    }
#else
    public class FactoryElement : ConfigurationElement
    {
        private const string NameAttributeName = "name";
        private const string MetaGraphUriAttributeName = "metaGraphUri";
        private const string BaseUrisName = "baseUris";
        private const string MappingAssembliesElementName = "mappingAssemblies";
        private const string OntologiesElementName = "ontologies";
        private const string ThreadSafeAttributeName = "threadSafe";
        private const string TrackChangesAttributeName = "trackChanges";

        /// <summary>Gets or sets the name.</summary>
        [ConfigurationProperty(NameAttributeName, IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)this[NameAttributeName]; }
            set { this[NameAttributeName] = value; }
        }

        /// <summary>Gets or sets a flag indicating whether the internal mechanisms should be thread-safe.</summary>
        [ConfigurationProperty(ThreadSafeAttributeName, DefaultValue = false)]
        public bool ThreadSafe
        {
            get { return (bool)this[ThreadSafeAttributeName]; }
            set { this[ThreadSafeAttributeName] = value; }
        }

        /// <summary>Gets or sets a flag indicating whether the changes should be tracked.</summary>
        [ConfigurationProperty(TrackChangesAttributeName, DefaultValue = true)]
        public bool TrackChanges
        {
            get { return (bool)this[TrackChangesAttributeName]; }
            set { this[TrackChangesAttributeName] = value; }
        }

        /// <summary>Gets or sets the mapping assemblies.</summary>
        [ConfigurationProperty(MappingAssembliesElementName)]
        [ConfigurationCollection(typeof(MappingAssembliesCollection))]
        public MappingAssembliesCollection MappingAssemblies
        {
            get { return (MappingAssembliesCollection)this[MappingAssembliesElementName]; }
            set { this[MappingAssembliesElementName] = value; }
        }

        /// <summary>Gets or sets the ontologies configuration element collection.</summary>
        [ConfigurationProperty(OntologiesElementName)]
        [ConfigurationCollection(typeof(OntologiesCollection))]
        public OntologiesCollection Ontologies
        {
            get { return (OntologiesCollection)this[OntologiesElementName]; }
            set { this[OntologiesElementName] = value; }
        }

        /// <summary>Gets or sets the base uri configuration element.</summary>
        [ConfigurationProperty(BaseUrisName)]
        public BaseUriElement BaseUris
        {
            get { return (BaseUriElement)this[BaseUrisName]; }
            set { this[BaseUrisName] = value; }
        }

        /// <summary>Gets or sets the meta graph URI.</summary>
        [ConfigurationProperty(MetaGraphUriAttributeName, IsRequired = true)]
        [UriValidator]
        public Uri MetaGraphUri
        {
            get { return (Uri)this[MetaGraphUriAttributeName]; }
            set { this[MetaGraphUriAttributeName] = value; }
        }
    }
#endif
}