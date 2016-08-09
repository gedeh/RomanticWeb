using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using RomanticWeb.Entities;

namespace RomanticWeb.Model
{
    internal sealed class EntityQuadCollection : IEntityQuadCollection
    {
        private readonly bool _threadSafe;
        private readonly IDictionary<int, IEntityQuad> _quads;
        private readonly IDictionary<EntityId, IDictionary<int, IEntityQuad>> _entityQuads;
        private readonly IDictionary<INode, IDictionary<int, IEntityQuad>> _subjectQuads;
        private readonly IDictionary<Tuple<INode, INode>, IDictionary<IEntityQuad, IEntityQuad>> _subjectPredicateQuads;
        private readonly IDictionary<INode, IDictionary<int, EntityId>> _objectIndex;

        internal EntityQuadCollection(bool threadSafe)
        {
            if (_threadSafe = threadSafe)
            {
                _quads = new Dictionary<int, IEntityQuad>();
                _entityQuads = new Dictionary<EntityId, IDictionary<int, IEntityQuad>>();
                _subjectQuads = new Dictionary<INode, IDictionary<int, IEntityQuad>>();
                _subjectPredicateQuads = new Dictionary<Tuple<INode, INode>, IDictionary<IEntityQuad, IEntityQuad>>();
                _objectIndex = new Dictionary<INode, IDictionary<int, EntityId>>();
            }
            else
            {
                _quads = new ConcurrentDictionary<int, IEntityQuad>();
                _entityQuads = new ConcurrentDictionary<EntityId, IDictionary<int, IEntityQuad>>();
                _subjectQuads = new ConcurrentDictionary<INode, IDictionary<int, IEntityQuad>>();
                _subjectPredicateQuads = new ConcurrentDictionary<Tuple<INode, INode>, IDictionary<IEntityQuad, IEntityQuad>>();
                _objectIndex = new ConcurrentDictionary<INode, IDictionary<int, EntityId>>();
            }
        }

        public int Count { get { return _quads.Count; } }

        public bool IsReadOnly { get { return false; } }

        IEnumerable<IEntityQuad> IEntityQuadCollection.this[EntityId entityId] { get { return EntityQuads(entityId).Values; } }

        IEnumerable<IEntityQuad> IEntityQuadCollection.this[INode entityId] { get { return SubjectQuads(entityId).Values; } }

        IEnumerable<IEntityQuad> IEntityQuadCollection.this[INode entityId, INode predicate] { get { return SubjectPredicateQuads(entityId, predicate).Values; } }

        public IEnumerable<IEntityQuad> RemoveWhereObject(INode obj)
        {
            if (!_objectIndex.ContainsKey(obj))
            {
                return new IEntityQuad[0];
            }

            var toRemove = from cotainingEntity in _objectIndex[obj]
                           from quadWithSubject in SubjectQuads(Node.FromEntityId(cotainingEntity.Value))
                           where quadWithSubject.Value.Object.Equals(obj)
                           select quadWithSubject.Value;

            return toRemove.ToList().Where(Remove).ToList();
        }

        public void Add(EntityId entityId, IEnumerable<IEntityQuad> entityQuads)
        {
            foreach (var entityQuad in entityQuads)
            {
                Add(entityQuad);
            }
        }

        public void Add(IEntityQuad quad)
        {
            var entity = GetEntityId(quad.EntityId);

            _quads[quad.GetHashCode()] = quad;
            EntityQuads(entity)[quad.GetHashCode()] = quad;
            EntityQuads(quad.EntityId)[quad.GetHashCode()] = quad;
            SubjectQuads(quad.Subject)[quad.GetHashCode()] = quad;
            SubjectPredicateQuads(quad.Subject, quad.Predicate)[quad] = quad;
            ObjectIndex(quad.Object)[quad.EntityId.GetHashCode()] = quad.EntityId;
        }

        public void Clear()
        {
            _quads.Clear();
            _entityQuads.Clear();
            _subjectQuads.Clear();
            _subjectPredicateQuads.Clear();
            _objectIndex.Clear();
        }

        public bool Contains(IEntityQuad item)
        {
            return _quads.ContainsKey(item.GetHashCode());
        }

        public void CopyTo(IEntityQuad[] array, int arrayIndex)
        {
            _quads.Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(IEntityQuad entityTriple)
        {
            var entity = GetEntityId(entityTriple.EntityId);
            EntityQuads(entityTriple.EntityId).Remove(entityTriple.GetHashCode());
            EntityQuads(entity).Remove(entityTriple.GetHashCode());
            SubjectQuads(entityTriple.Subject).Remove(entityTriple.GetHashCode());
            SubjectPredicateQuads(entityTriple.Subject, entityTriple.Predicate).Remove(entityTriple);
            return _quads.Remove(entityTriple.GetHashCode());
        }

        IEnumerator<IEntityQuad> IEnumerable<IEntityQuad>.GetEnumerator()
        {
            return _quads.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _quads.Values.GetEnumerator();
        }

        private static EntityId GetEntityId(EntityId entityId)
        {
            var blankId = entityId as BlankId;
            while (blankId != null)
            {
                entityId = blankId.RootEntityId;
                blankId = entityId as BlankId;
            }

            return entityId;
        }

        private IDictionary<int, IEntityQuad> EntityQuads(EntityId entityId)
        {
            if (!_entityQuads.ContainsKey(entityId))
            {
                _entityQuads[entityId] = (_threadSafe ? (IDictionary<int, IEntityQuad>)new ConcurrentDictionary<int, IEntityQuad>() : new Dictionary<int, IEntityQuad>());
            }

            return _entityQuads[entityId];
        }

        private IDictionary<int, IEntityQuad> SubjectQuads(INode subject)
        {
            if (!_subjectQuads.ContainsKey(subject))
            {
                _subjectQuads[subject] = (_threadSafe ? (IDictionary<int, IEntityQuad>)new ConcurrentDictionary<int, IEntityQuad>() : new Dictionary<int, IEntityQuad>());
            }

            return _subjectQuads[subject];
        }

        private IDictionary<IEntityQuad, IEntityQuad> SubjectPredicateQuads(INode entityId, INode predicate)
        {
            var key = Tuple.Create(entityId, predicate);
            if (!_subjectPredicateQuads.ContainsKey(key))
            {
                _subjectPredicateQuads[key] = (_threadSafe ?
                    (IDictionary<IEntityQuad, IEntityQuad>)new ConcurrentDictionary<IEntityQuad, IEntityQuad>(LooseEntityQuadEqualityComparer.Instance) :
                    new Dictionary<IEntityQuad, IEntityQuad>(LooseEntityQuadEqualityComparer.Instance));
            }

            return _subjectPredicateQuads[key];
        }

        private IDictionary<int, EntityId> ObjectIndex(INode entityId)
        {
            if (!_objectIndex.ContainsKey(entityId))
            {
                _objectIndex[entityId] = (_threadSafe ? (IDictionary<int, EntityId>)new ConcurrentDictionary<int, EntityId>() : new Dictionary<int, EntityId>());
            }

            return _objectIndex[entityId];
        }
    }
}