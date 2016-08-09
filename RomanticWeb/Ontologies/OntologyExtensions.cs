namespace RomanticWeb.Ontologies
{
    internal static class OntologyExtensions
    {
        internal static T InOntology<T>(this T term, IOntology ontology) where T : Term
        {
            term.Ontology = ontology;
            return term;
        }
    }
}