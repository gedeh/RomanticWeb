using System;

namespace RomanticWeb.Model
{
    /// <summary>Reprents a triple, which does nto belong to a graph.</summary>
    public interface ITriple : IComparable, IComparable<ITriple>
    {
        /// <summary>Gets the triple's object.</summary>
        INode Object { get; }

        /// <summary>Gets the triple's predicate.</summary>
        INode Predicate { get; }

        /// <summary>Gets the triple's subject.</summary>
        INode Subject { get; }
    }
}