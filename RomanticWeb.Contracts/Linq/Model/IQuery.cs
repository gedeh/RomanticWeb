using System.Collections.Generic;

namespace RomanticWeb.Linq.Model
{
    /// <summary>Specifies target SPARQL query form.</summary>
    public enum QueryForms
    {
        /// <summary>Selects expressions in to the resulting data set.</summary>
        Select,

        /// <summary>Asks for existance of a given entities.</summary>
        Ask,

        /// <summary>Returns triples describing given entities.</summary>
        Describe,

        /// <summary>Returns triples that can be used to construct another triple store.</summary>
        Construct
    }

    /// <summary>Represents a whole query.</summary>
    public interface IQuery : IQueryElement
    {
        /// <summary>Gets an enumeration of all prefixes.</summary>
        IList<IPrefix> Prefixes { get; }

        /// <summary>Gets an enumeration of all selected expressions.</summary>
        IList<ISelectableQueryComponent> Select { get; }

        /// <summary>Gets an enumeration of all query elements.</summary>
        IList<IQueryElement> Elements { get; }

        /// <summary>Gets a value indicating if the given query is actually a sub query.</summary>
        bool IsSubQuery { get; }

        /// <summary>Gets a query form of given query.</summary>
        QueryForms QueryForm { get; }

        /// <summary>Gets or sets the offset.</summary>
        int Offset { get; set; }

        /// <summary>Gets or sets the limit.</summary>
        int Limit { get; set; }

        /// <summary>Gets a map of order by clauses.</summary>
        /// <remarks>Key is the expression on which the sorting should be performed and the value determines the direction, where <b>true</b> means descending and <b>false</b> is for ascending (default).</remarks>
        IDictionary<IExpression, bool> OrderBy { get; }

        /// <summary>Creates a variable name from given identifier.</summary>
        /// <param name="identifier">Identifier to be used to abbreviate variable name.</param>
        /// <returns>Variable name with unique name.</returns>
        string CreateVariableName(string identifier);

        /// <summary>Retrieves an identifier from a passed variable name.</summary>
        /// <param name="variableName">Variable name to retrieve identifier from.</param>
        /// <returns>Identifier passed to create the variable name.</returns>
        string RetrieveIdentifier(string variableName);

        /// <summary>Creates an identifier from given name.</summary>
        /// <param name="name">Name.</param>
        /// <returns>Identifier created from given name.</returns>
        string CreateIdentifier(string name);
    }
}