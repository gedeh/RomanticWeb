using System;

namespace RomanticWeb.Ontologies
{
    /// <summary>Base class for RDF terms (properties and classes).</summary>
    public interface ITerm
    {
        /// <summary>Gets the <see cref="ITerm"/>'s URI.</summary>
        Uri Uri { get; }

        /// <summary>Gets the <see cref="IOntology"/>, which defines this <see cref="ITerm"/>.</summary>
        IOntology Ontology { get; }

        /// <summary>Gets the prefix of this term.</summary>
        string Prefix { get; }

        /// <summary>Gets the term name.</summary>
        /// <remarks>Essentially it is a relative URI or hash part (depending on ontology namespace)</remarks>
        string Name { get; }
    }
}