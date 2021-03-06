namespace RomanticWeb.Entities
{
    /// <summary>Defines a contract for entities identified by <see cref="EntityId"/>.</summary>
    public interface IEntity
    {
        /// <summary>Gets the entity's identifier.</summary>
        EntityId Id { get; }

        /// <summary>Gets the entity context.</summary>
        IEntityContext Context { get; }
    }
}