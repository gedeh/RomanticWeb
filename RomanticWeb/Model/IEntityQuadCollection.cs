using System.Collections.Generic;
using RomanticWeb.Entities;

namespace RomanticWeb.Model
{
    internal interface IEntityQuadCollection : ICollection<IEntityQuad>
    {
        IEnumerable<IEntityQuad> this[EntityId entityId] { get; }
        
        IEnumerable<IEntityQuad> this[INode entityId] { get; }

        IEnumerable<IEntityQuad> this[INode entityId, INode predicate] { get; }

        IEnumerable<IEntityQuad> RemoveWhereObject(INode entityId);

        void Add(EntityId entityId, IEnumerable<IEntityQuad> entityQuads);
    }
}