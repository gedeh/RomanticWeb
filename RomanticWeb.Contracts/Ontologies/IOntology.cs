using System;
using System.Collections.Generic;

namespace RomanticWeb.Ontologies
{
    /// <summary>Encapsulates metadata about an ontology (like Foaf, Dublin Core, Rdfs, etc.).</summary>
    public interface IOntology
    {
        /// <summary>Gets the namespace prefix.</summary>
        string Prefix { get; }

        /// <summary>Gets the display name.</summary>
        /// <remarks>This property is usually fed with dc:title or rdfs:label property.</remarks>
        string DisplayName { get; }

        /// <summary>Gets the ontology's base URI.</summary>
        Uri BaseUri { get; }

        /// <summary>Gets the ontology's properties.</summary>
        IEnumerable<IProperty> Properties { get; }

        /// <summary>Gets the ontology's classes.</summary>
        IEnumerable<IClass> Classes { get; }
    }
}