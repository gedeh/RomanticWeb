#if !NETSTANDARD1_6
using NUnit.Framework;
using RomanticWeb.DotNetRDF;
using VDS.RDF;

namespace RomanticWeb.Tests.IntegrationTests.Virtuoso
{
    [TestFixture]
    public class LoadingTests : LoadingTestsBase
    {
        private ITripleStore _store;

        protected ITripleStore Store
        {
            get
            {
                if (_store == null)
                {
                    _store = new PersistentTripleStore(new VirtuosoManager("DB", "dba", "dba"));
                }

                return _store;
            }
        }

        protected override void LoadTestFile(string fileName)
        {
        }

        protected override IEntitySource CreateEntitySource()
        {
            return new TripleStoreAdapter();
        }

        protected override void ChildTeardown()
        {
            _store = null;
        }
    }
}
#endif