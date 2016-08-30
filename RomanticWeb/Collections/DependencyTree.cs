using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RomanticWeb.Collections
{
    internal class DependencyTree<T> : IEnumerable<T> where T : IDependentComponent
    {
        private readonly IList<DependencyNode> _dependencies;

        internal DependencyTree(IEnumerable<T> instances)
        {
            _dependencies = new List<DependencyNode>();
            var visited = new List<T>();
            foreach (var instance in instances)
            {
                if (visited.Contains(instance))
                {
                    continue;
                }

                _dependencies.Add(new DependencyNode(instances, instance, visited));
            }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return new DependencyTreeEnumerator(_dependencies);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class DependencyTreeEnumerator : IEnumerator<T>
        {
            private readonly IList<DependencyNode> _dependencies;
            private readonly IList<T> _visited;

            internal DependencyTreeEnumerator(IList<DependencyNode> dependencies)
            {
                _dependencies = dependencies;
                _visited = new List<T>();
            }

            /// <inheritdoc />
            public T Current { get; private set; }

            /// <inheritdoc />
            object IEnumerator.Current { get { return Current; } }

            /// <inheritdoc />
            public bool MoveNext()
            {
                var result = FindNext(_dependencies, null);
                if (result == null)
                {
                    return false;
                }

                _visited.Add(Current = result.Instance);
                return true;
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }

            /// <inheritdoc />
            public void Reset()
            {
                _visited.Clear();
            }

            private DependencyNode FindNext(IList<DependencyNode> dependencies, DependencyNode current)
            {
                foreach (var dependency in dependencies)
                {
                    if ((current != null) && (_visited.Contains(current.Instance)))
                    {
                        continue;
                    }

                    if (dependency.Dependencies.Count > 0)
                    {
                        DependencyNode result = FindNext(dependency.Dependencies, dependency);
                        if (result != null)
                        {
                            return result;
                        }
                    }

                    if (!_visited.Contains(dependency.Instance))
                    {
                        return dependency;
                    }
                }

                return null;
            }
        }

        private class DependencyNode
        {
            internal DependencyNode(IEnumerable<T> instances, T instance, IList<T> visited)
            {
                Instance = instance;
                Dependencies = new List<DependencyNode>();
                foreach (var dependency in instance.Requires)
                {
                    var matchingInstance = (from otherInstance in instances
                                            where dependency.IsInstanceOfType(otherInstance)
                                            select otherInstance).FirstOrDefault();
                    if ((matchingInstance == null) || (visited.Contains(matchingInstance)))
                    {
                        continue;
                    }

                    visited.Add(matchingInstance);
                    Dependencies.Add(new DependencyNode(instances, matchingInstance, visited));
                }
            }

            internal IList<DependencyNode> Dependencies { get; private set; }

            internal T Instance { get; private set; }
        }
    }
}