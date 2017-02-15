using System.Collections.Generic;
using RomanticWeb.Entities;
using RomanticWeb.TestEntities;

namespace RomanticWeb.Tests.Stubs
{
    /// <summary>Provides test properties.</summary>
    public class TestPropertyInfo
    {
        /// <summary>Gets or sets an integer enumeration.</summary>
        public IEnumerable<int> IntegerEnumeration { get; set; }

        /// <summary>Gets or sets an integer collection.</summary>
        public ICollection<int> IntegerCollection { get; set; }

        /// <summary>Gets or sets an integer array.</summary>
        public int[] IntegerArray { get; set; }

        /// <summary>Gets or sets an integer list.</summary>
        public IList<int> IntegerList { get; set; }

        /// <summary>Gets or sets an integer set.</summary>
        public ISet<int> IntegerSet { get; set; }

        /// <summary>Gets or sets an integer-string dictionary.</summary>
        public IDictionary<int, string> IntegerStringDictionary { get; set; }

        /// <summary>Gets or sets an integer-integer dictionary.</summary>
        public IDictionary<int, int> IntegerIntegerDictionary { get; set; }

        /// <summary>Gets or sets a floating point operation value.</summary>
        public float Float { get; set; }

        /// <summary>Gets or sets an integer value.</summary>
        public int Integer { get; set; }

        /// <summary>Gets or sets an integer value if any.</summary>
        public int? NullableInteger { get; set; }

        /// <summary>Gets or sets a test entity.</summary>
        public ITestEntity TestEntity { get; set; }

        /// <summary>Gets or sets a collection of test entities.</summary>
        public IList<ITestEntity> TestEntityList { get; set; }

        /// <summary>Gets or sets a person.</summary>
        public IPerson Person { get; set; }

        /// <summary>Gets or sets an entity.</summary>
        public IEntity Entity { get; set; }

        /// <summary>Gets or sets a list of entities.</summary>
        public IList<IEntity> EntityList { get; set; }
    }
}
