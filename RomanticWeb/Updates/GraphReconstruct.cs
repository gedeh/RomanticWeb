using System.Collections.Generic;
using System.Linq;
using RomanticWeb.Entities;
using RomanticWeb.Model;

namespace RomanticWeb.Updates
{
    /// <summary>Represents a change which recreates a named graph.</summary>
    public class GraphReconstruct : DatasetChange
    {
        private readonly ISet<IEntityQuad> _addedQuads;

        internal GraphReconstruct(EntityId entity, EntityId graph, IEnumerable<IEntityQuad> addedQuads) : base(entity, graph)
        {
            _addedQuads = new HashSet<IEntityQuad>(addedQuads);
        }

        /// <summary>Gets recreated graph's content.</summary>
        public IEnumerable<IEntityQuad> AddedQuads { get { return _addedQuads; } }

        /// <summary>Checks if the graph reconstruction is required.</summary>
        public override bool IsEmpty { get { return _addedQuads.Count == 0; } }

        /// <summary>Returns a description of the change.</summary>
        public override string ToString()
        {
            return string.Format("Recreating graph {0} with {1} triples", Graph, AddedQuads.Count());
        }

        /// <inheritdoc />
        public override bool CanMergeWith(IDatasetChange other)
        {
            return (other is GraphUpdate || other is GraphReconstruct) && base.CanMergeWith(other);
        }

        /// <inheritdoc />
        public override IDatasetChange MergeWith(IDatasetChange other)
        {
            return MergeWith((dynamic)other);
        }

        private DatasetChange MergeWith(GraphUpdate update)
        {
            _addedQuads.ExceptWith(update.RemovedQuads);
            _addedQuads.UnionWith(update.AddedQuads);

            return this;
        }

        private DatasetChange MergeWith(GraphReconstruct reconstruct)
        {
            return reconstruct;
        }
    }
}