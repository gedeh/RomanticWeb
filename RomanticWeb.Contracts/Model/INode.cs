using System;
using RomanticWeb.Entities;

namespace RomanticWeb.Model
{
    /// <summary>Represents an RDF node (URI or literal).</summary>
    /// <remarks>Blank nodes are not supported currently.</remarks>
    public interface INode : IComparable, IComparable<INode>, IEquatable<INode>
    {
        /// <summary>Gets the value indicating that the node is a URI.</summary>
        bool IsUri { get; }

        /// <summary>Gets the value indicating that the node is a literal.</summary>
        bool IsLiteral { get; }

        /// <summary>Gets the value indicating that the node is a blank node.</summary>
        bool IsBlank { get; }

        /// <summary>Gets the URI of a URI node.</summary>
        Uri Uri { get; }

        /// <summary>Gets the string value of a literal node.</summary>
        string Literal { get; }

        /// <summary>Gets the string value of a blank node.</summary>
        string BlankNode { get; }

        /// <summary>Gets the data type of a literal node.</summary>
        Uri DataType { get; }

        /// <summary>Gets the language tag of a literal node.</summary>
        string Language { get; }

        /// <summary>Creates an <see cref="EntityId"/> for a <see cref="INode"/>.</summary>
        EntityId ToEntityId();

        /// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
        /// <param name="nQuadFormat">If set to <c>true</c> the string will be a valid NQuad node.</param>
        string ToString(bool nQuadFormat);
    }
}