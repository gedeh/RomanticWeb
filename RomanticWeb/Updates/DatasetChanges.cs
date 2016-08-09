using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RomanticWeb.Entities;

namespace RomanticWeb.Updates
{
    /// <summary>Represents ordered changes made in the triple store.</summary>
    public sealed class DatasetChanges : IDatasetChangesTracker
    {
        private const int GraphChangesCapacity = 16;
        private readonly object _syncLock = new object();
        private readonly List<IDatasetChange> _frozenChanges = new List<IDatasetChange>(16);
        private readonly IDictionary<EntityId, Stack<IDatasetChange>> _graphChanges = new Dictionary<EntityId, Stack<IDatasetChange>>();

        /// <inheritdoc/>
        public bool HasChanges
        {
            get
            {
                return _graphChanges.Any() && _graphChanges.All(changes => changes.Value.Any());
            }
        }

        private IEnumerable<IDatasetChange> CurrentChanges
        {
            get
            {
                return _graphChanges.SelectMany(changes => changes.Value);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IDatasetChange> this[EntityId graphUri]
        {
            get
            {
                return _graphChanges[graphUri];
            }
        }

        /// <inheritdoc/>
        public void Add(IDatasetChange datasetChange)
        {
            if (datasetChange.IsEmpty)
            {
                return;
            }

            if (datasetChange.Graph == null)
            {
                FreezeCurrentChanges();
                _frozenChanges.Add(datasetChange);
            }
            else
            {
                AppendAndMerge(datasetChange);
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _graphChanges.Clear();
            _frozenChanges.Clear();
        }

        /// <summary>Gets the enumerator of changes.</summary>
        public IEnumerator<IDatasetChange> GetEnumerator()
        {
            return _frozenChanges.Union(CurrentChanges).GetEnumerator();
        }

        /// <summary>Gets the enumerator of changes grouped by named graphs.</summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private Stack<IDatasetChange> ChangesFor(EntityId graph)
        {
            if (!_graphChanges.ContainsKey(graph))
            {
                _graphChanges[graph] = new Stack<IDatasetChange>(GraphChangesCapacity);
            }

            return _graphChanges[graph];
        }

        private void FreezeCurrentChanges()
        {
            _frozenChanges.AddRange(CurrentChanges);
        }

        private void AppendAndMerge(IDatasetChange datasetChange)
        {
            var nextChange = datasetChange;

            lock (_syncLock)
            {
                var current = ChangesFor(datasetChange.Graph);

                if (current.Count > 0 && current.Peek().CanMergeWith(nextChange))
                {
                    var previousChange = current.Pop();
                    nextChange = previousChange.MergeWith(nextChange);
                }

                current.Push(nextChange);
            }
        }
    }
}