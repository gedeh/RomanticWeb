using System.Diagnostics.CodeAnalysis;

namespace RomanticWeb.Ontologies
{
    /// <summary>A base classs for RDF properties.</summary>
    public class Property : Term, IProperty
    {
        /// <summary>Creates a new Property. </summary>
        internal Property(string predicateName) : base(predicateName)
        {
        }

#pragma warning disable 1591
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return System.String.Format("{0}:{1}", Ontology == null ? "_" : Ontology.Prefix, Name);
        }
#pragma warning restore
    }
}