using RomanticWeb.Entities;

namespace RomanticWeb.Updates
{
    /// <summary>Represents a change to the triple store.</summary>
    public interface IDatasetChange
    {
        /// <summary>Gets the entity, which was changed.</summary>
        EntityId Entity { get; }

        /// <summary>Gets the graph, which was changed.</summary>
        /// <returns><b>null</b> if change affects multiple graphs.</returns>
        EntityId Graph { get; }

        /// <summary>Gets a value indicating whether this instance actually represents a change to the store.</summary>
        bool IsEmpty { get; }

        /// <summary>Determines whether this instance can be merged with another.</summary>
        /// <param name="other">The other change.</param>
        bool CanMergeWith(IDatasetChange other);

        /// <summary>Merges this change the with another change.</summary>
        IDatasetChange MergeWith(IDatasetChange other);
    }
}