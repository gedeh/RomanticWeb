using System.Collections.Generic;

namespace RomanticWeb.Model
{
    internal class LooseEntityQuadEqualityComparer : IEqualityComparer<IEntityQuad>
    {
        internal static readonly LooseEntityQuadEqualityComparer Instance = new LooseEntityQuadEqualityComparer();

        private LooseEntityQuadEqualityComparer()
        {
        }

        public bool Equals(IEntityQuad x, IEntityQuad y)
        {
            return GetHashCode(x).Equals(GetHashCode(y));
        }

        public int GetHashCode(IEntityQuad obj)
        {
            unchecked
            {
                var hashCode = obj.Object.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Subject.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Predicate.GetHashCode();
                return hashCode;
            }
        }
    }
}