using System.Diagnostics.CodeAnalysis;

namespace RomanticWeb.Ontologies
{
    /// <summary>Represents an RDF class.</summary>
    public class Class : Term, IClass
    {
        /// <summary>Creates a new instance of <see cref="Class"/>.</summary>
        /// <param name="className">Name of the class.</param>
        public Class(string className) : base(className)
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