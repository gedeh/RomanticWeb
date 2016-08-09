using System;
using System.Collections.Generic;

namespace RomanticWeb.Ontologies
{
    /// <summary>Defines methods for accessing metadata about ontologies.</summary>
    public interface IOntologyProvider
    {
        /// <summary>Get ontologies' metadata.</summary>
        IEnumerable<IOntology> Ontologies { get; }

        /// <summary>Gets a URI from a QName.</summary>
        Uri ResolveUri(string prefix, string rdfTermName);
    }
}