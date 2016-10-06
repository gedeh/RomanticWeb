using RomanticWeb.Linq.Model;
using RomanticWeb.Linq.Sparql;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace RomanticWeb.DotNetRDF
{
    /// <summary>Represents an abstract SPARQL command execution strategy.</summary>
    public interface ISparqlCommandExecutionStrategy
    {
        /// <summary>Transforms a given SPARQL query into an dotNetRDF model.</summary>
        /// <param name="sparqlQuery">SPARQL query to be transformed.</param>
        /// <returns>SPARQL command in dotNetRDF model.</returns>
        SparqlQuery GetSparqlQuery(IQuery sparqlQuery);

        /// <summary>Transforms a given SPARQL query into an dotNetRDF model.</summary>
        /// <param name="sparqlQuery">SPARQL query to be transformed.</param>
        /// <param name="variables">Container that will carry out variable mappings.</param>
        /// <returns>SPARQL command in dotNetRDF model.</returns>
        SparqlQuery GetSparqlQuery(IQuery sparqlQuery, out SparqlQueryVariables variables);

        /// <summary>Executes a given SPARQL update command set.</summary>
        /// <param name="commands">Commands to be executed.</param>
        void ExecuteCommandSet(SparqlUpdateCommandSet commands);

        /// <summary>Executes an SPARQL ASK query.</summary>
        /// <param name="sparqlQuery">Target SPARQL query to be transformed.</param>
        /// <returns><b>true</b> if the ASK query evaluates so; otherwise <b>false</b>.</returns>
        bool ExecuteAsk(IQuery sparqlQuery);

        /// <summary>Executes an SPARQL ASK query.</summary>
        /// <param name="sparqlQuery">Target SPARQL query to be transformed.</param>
        /// <returns><b>true</b> if the ASK query evaluates so; otherwise <b>false</b>.</returns>
        bool ExecuteAsk(SparqlQuery sparqlQuery);

        /// <summary>Executes a SPARQL SELECT query.</summary>
        /// <param name="sparqlQuery">Target query to be executed.</param>
        /// <returns>Result set.</returns>
        SparqlResultSet ExecuteSelect(SparqlQuery sparqlQuery);

        /// <summary>Executes a SPARQL SELECT query.</summary>
        /// <param name="sparqlQuery">Target query to be executed.</param>
        /// <returns>Result set.</returns>
        SparqlResultSet ExecuteSelect(IQuery sparqlQuery);

        /// <summary>Executes a SPARQL SELECT query.</summary>
        /// <param name="sparqlQuery">Target query to be executed.</param>
        /// <param name="variables">Container that will carry out variable mappings.</param>
        /// <returns>Result set.</returns>
        SparqlResultSet ExecuteSelect(IQuery sparqlQuery, out SparqlQueryVariables variables);
    }
}