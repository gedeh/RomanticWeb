using System;
using RomanticWeb.Entities;

namespace RomanticWeb.Model
{
    /// <summary>Represents a triple (subject, predicate, object).</summary>
    public interface IEntityQuad : ITriple, IComparable<IEntityQuad>
    {
        /// <summary>Gets the named graph node or null, if triple is in named graph.</summary>
        INode Graph { get; }

        /// <summary>Gets entity id, which defines this triple.</summary>
        EntityId EntityId { get; }

        /// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
        /// <param name="nQuadFormat">If set to <c>true</c> the string will be a valid NQuad.</param>
        string ToString(bool nQuadFormat);
    }
}