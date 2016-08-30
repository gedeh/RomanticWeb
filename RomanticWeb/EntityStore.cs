using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RomanticWeb.Entities;
using RomanticWeb.Model;
using RomanticWeb.Updates;

namespace RomanticWeb
{
    internal class EntityStore : IThreadSafeEntityStore
    {
        private readonly IDatasetChangesTracker _changesTracker;
        private readonly ISet<EntityId> _assertedEntities = new HashSet<EntityId>();
        private readonly IDictionary<INode, int> _blankNodeRefCounts = new ConcurrentDictionary<INode, int>();
        private IEntityQuadCollection _entityQuads;
        private IEntityQuadCollection _initialQuads;
        private bool _disposed;
        private bool _threadSafe;
        private bool _trackChanges = true;

        public EntityStore(IDatasetChangesTracker changesTracker)
        {
            _changesTracker = changesTracker;
            _entityQuads = new EntityQuadCollection(_threadSafe);
            _initialQuads = new EntityQuadCollection(_threadSafe);
        }

        public bool ThreadSafe
        {
            get
            {
                return _threadSafe;
            }

            set
            {
                if (_threadSafe == value)
                {
                    return;
                }

                if ((_entityQuads.Count > 0) || (_initialQuads.Count > 0))
                {
                    throw new InvalidOperationException("Cannot set thread-safety flag. EntityStore already in use.");
                }

                _threadSafe = value;
                _entityQuads = new EntityQuadCollection(_threadSafe);
                _initialQuads = new EntityQuadCollection(_threadSafe);
            }
        }

        public IEnumerable<IEntityQuad> Quads { get { return _entityQuads; } }

        public bool TrackChanges
        {
            get
            {
                return _trackChanges;
            }

            set
            {
                if (_trackChanges == value)
                {
                    return;
                }

                if (!(_trackChanges = value))
                {
                    _initialQuads = null;
                    _changesTracker.Clear();
                }
                else
                {
                    _initialQuads = new EntityQuadCollection(_threadSafe);
                }
            }
        }

        public IDatasetChanges Changes { get { return _changesTracker; } }

        public IEnumerable<INode> GetObjectsForPredicate(EntityId entityId, Uri predicate, Uri graph)
        {
            var quads = _entityQuads[Node.FromEntityId(entityId), Node.ForUri(predicate)];

            if (graph != null)
            {
                quads = quads.Where(triple => GraphEquals(triple, graph));
            }

            return quads.Select(triple => triple.Object).ToList();
        }

        public IEnumerable<IEntityQuad> GetEntityQuads(EntityId entityId)
        {
            return _entityQuads[entityId];
        }

        public void AssertEntity(EntityId entityId, IEnumerable<IEntityQuad> entityTriples)
        {
            if (_assertedEntities.Contains(entityId))
            {
                return;
            }

            var entityQuads = entityTriples as IEntityQuad[] ?? entityTriples.ToArray();
            _entityQuads.Add(entityId, entityQuads);
            if (_trackChanges)
            {
                _initialQuads.Add(entityId, entityQuads);
            }

            _assertedEntities.Add(entityId);
            foreach (var entityQuad in entityQuads.Where(entityQuad => entityQuad.Object.IsBlank))
            {
                IncrementRefCount(entityQuad.Object);
            }
        }

        public void ReplacePredicateValues(EntityId entityId, INode propertyUri, Func<IEnumerable<INode>> newValues, Uri graphUri, CultureInfo language)
        {
            var subjectNode = Node.FromEntityId(entityId);
            var removedQuads = RemoveTriples(subjectNode, propertyUri, graphUri, language).ToArray();
            var newQuads = (from node in newValues()
                            select new EntityQuad(entityId, subjectNode, propertyUri, node).InGraph(graphUri)).ToArray();
            _entityQuads.Add(entityId, newQuads);

            foreach (var newQuad in newQuads.Where(q => q.Object.IsBlank))
            {
                IncrementRefCount(newQuad.Object);
            }

            DeleteOrphanedBlankNodes(removedQuads);

            if (_trackChanges)
            {
                dynamic datasetChange = CreateChangeForUpdate(entityId, graphUri, removedQuads, newQuads);
                _changesTracker.Add(datasetChange);
            }
        }

        public void Delete(EntityId entityId, DeleteBehaviour deleteBehaviour = DeleteBehaviour.Default)
        {
            IEnumerable<IEntityQuad> deletes = DeleteQuads(entityId);
            if (!_trackChanges)
            {
                return;
            }

            var deletesGrouped = (from removedQuad in deletes 
                                  group removedQuad by removedQuad.Graph into g 
                                  select g).ToList();

            if (entityId is BlankId)
            {
                foreach (var removed in deletesGrouped)
                {
                    if (removed.Any(quad => quad.Object.IsBlank || quad.Subject.IsBlank))
                    {
                        var removedQuads = removed;
                        _changesTracker.Add(new GraphReconstruct(entityId, removed.Key.ToEntityId(), _entityQuads.Where(q => (Equals(q.Graph, removedQuads.Key)) || (q.Graph.Equals(removedQuads.Key)))));
                    }
                }
            }
            else
            {
                _changesTracker.Add(new EntityDelete(entityId));

                if (deleteBehaviour.HasFlag(DeleteBehaviour.NullifyChildren))
                {
                    _entityQuads.RemoveWhereObject(Node.FromEntityId(entityId));
                    _changesTracker.Add(new RemoveReferences(entityId));
                }
            }
        }

        public void ResetState()
        {
            if (_trackChanges)
            {
                _initialQuads.Clear();

                foreach (var entityId in _entityQuads)
                {
                    _initialQuads.Add(entityId.EntityId, _entityQuads[entityId.EntityId]);
                }
            }

            _changesTracker.Clear();
        }

        public void Rollback()
        {
            _changesTracker.Clear();
            _entityQuads.Clear();
            if (_trackChanges)
            {
                foreach (var entityId in _initialQuads)
                {
                    _entityQuads.Add(entityId.EntityId, _initialQuads[entityId.EntityId]);
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _entityQuads.Clear();
            if (_trackChanges)
            {
                _initialQuads.Clear();
            }

            _disposed = true;
        }

        private IEnumerable<IEntityQuad> DeleteQuads(EntityId entityId)
        {
            var deletes = (from entityQuad in _entityQuads[Node.FromEntityId(entityId)].ToList()
                           from removedQuad in RemoveTriple(entityQuad)
                           select removedQuad).ToList();

            if (entityId is BlankId)
            {
                deletes.AddRange(_entityQuads.RemoveWhereObject(Node.FromEntityId(entityId)));
            }

            var orphanedBlankEntities = (from quad in deletes
                                         where quad.Object.IsBlank && !IsReferenced(quad.Object)
                                         select quad).ToArray();
            foreach (var quad in orphanedBlankEntities)
            {
                deletes.AddRange(DeleteQuads(quad.Object.ToEntityId()));
            }

            return deletes;
        } 

        private DatasetChange CreateChangeForUpdate(EntityId entityId, EntityId graphUri, IEntityQuad[] removedQuads, IEntityQuad[] addedQuads)
        {
            var update = new GraphUpdate(entityId, graphUri, removedQuads, addedQuads);

            if (update.RemovedQuads.Any(q => q.Subject.IsBlank || q.Object.IsBlank))
            {
                var graphQuads = from entityQuad in GetEntityQuads(entityId is BlankId ? ((BlankId)entityId).RootEntityId : entityId)
                                 where entityQuad.Graph.Equals(Node.FromEntityId(graphUri))
                                 select entityQuad;
                return new GraphReconstruct(entityId, graphUri, graphQuads);
            }

            return update;
        }

        private void DeleteOrphanedBlankNodes(IEnumerable<IEntityQuad> removedQuads)
        {
            var orphanedBlankNodes = from removedQuad in removedQuads
                                     where removedQuad.Object.IsBlank
                                     where !IsReferenced(removedQuad.Object)
                                     select removedQuad.Object;

            foreach (var orphan in orphanedBlankNodes)
            {
                Delete(orphan.ToEntityId());
            }
        }

        /// <summary>Removes triple and blank node's subgraph if present.</summary>
        /// <returns>a value indicating that the was a blank node object value</returns>
        private IEnumerable<IEntityQuad> RemoveTriples(INode entityId, INode predicate = null, Uri graphUri = null, CultureInfo language = null)
        {
            var quadsRemoved = (predicate == null ? _entityQuads[entityId] : _entityQuads[entityId, predicate]);
            if (graphUri != null)
            {
                quadsRemoved = quadsRemoved.Where(quad => GraphEquals(quad, graphUri));
            }

            if (language != null)
            {
                quadsRemoved = quadsRemoved.Where(quad => ((!quad.Object.IsLiteral) || ((quad.Object.IsLiteral) && 
                    (((quad.Object.Language == null) && (CultureInfo.InvariantCulture.Equals(language))) || (quad.Object.Language == language.TwoLetterISOLanguageName)))));
            }

            return quadsRemoved.ToList().SelectMany(RemoveTriple);
        }

        private IEnumerable<IEntityQuad> RemoveTriple(IEntityQuad entityTriple)
        {
            if (_entityQuads.Remove(entityTriple) && entityTriple.Object.IsBlank)
            {
                DecrementRefCount(entityTriple.Object);
            }

            yield return entityTriple;
        }

        // TODO: Make the GraphEquals method a bit less rigid.
        private bool GraphEquals(IEntityQuad triple, Uri graph)
        {
            return (triple.Graph != null) && ((triple.Graph.Uri.AbsoluteUri == graph.AbsoluteUri) || 
                ((triple.Subject.IsBlank) && (graph.AbsoluteUri.EndsWith(triple.Graph.Uri.AbsoluteUri))));
        }

        private void DecrementRefCount(INode node)
        {
            AssertIsBlankNode(node);
            _blankNodeRefCounts[node] -= 1;
        }

        private void IncrementRefCount(INode node)
        {
            AssertIsBlankNode(node);

            if (!_blankNodeRefCounts.ContainsKey(node))
            {
                _blankNodeRefCounts[node] = 0;
            }

            _blankNodeRefCounts[node] += 1;
        }

        private bool IsReferenced(INode node)
        {
            AssertIsBlankNode(node);

            return _blankNodeRefCounts.ContainsKey(node) && _blankNodeRefCounts[node] > 0;
        }

        private void AssertIsBlankNode(INode node)
        {
            if (!node.IsBlank)
            {
                throw new ArgumentOutOfRangeException("node", "Must be blank node");
            }
        }
    }
}