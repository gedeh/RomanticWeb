using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using RomanticWeb.DotNetRDF;
using RomanticWeb.Updates;
using VDS.RDF;
using VDS.RDF.Update;

namespace RomanticWeb.Tests.DotNetRDF
{
    [TestFixture]
    public class TripleStoreAdapterTests
    {
        private static readonly Uri MetaGraphUri = new Uri("urn:meta:graph");
        private TripleStoreAdapter _tripleStore;
        private Mock<IUpdateableTripleStore> _realStore;
        private Mock<ISparqlCommandFactory> _factory;
        private Mock<ISparqlCommandExecutionStrategyFactory> _strategyFactory;
        private Mock<ISparqlCommandExecutionStrategy> _strategy;

        [SetUp]
        public void Setup()
        {
            _realStore = new Mock<IUpdateableTripleStore>();
            _factory = new Mock<ISparqlCommandFactory>(MockBehavior.Strict);
            _strategy = new Mock<ISparqlCommandExecutionStrategy>(MockBehavior.Strict);
            _strategyFactory = new Mock<ISparqlCommandExecutionStrategyFactory>(MockBehavior.Strict);
            _strategyFactory.Setup(instance => instance.CreateFor(_realStore.Object, MetaGraphUri)).Returns(_strategy.Object);

            _tripleStore = new TripleStoreAdapter(_realStore.Object, _factory.Object, _strategyFactory.Object) { MetaGraphUri = MetaGraphUri };
        }

        [Test]
        public void Should_convert_commands_and_execute_on_triple_store()
        {
            // given
            var testCommand = new TestCommand();
            var testUpdates = new TestUpdate();
            _factory.Setup(f => f.CreateCommands(testUpdates)).Returns(new[] { testCommand });
            _strategy.Setup(instance => instance.ExecuteCommandSet(It.IsAny<SparqlUpdateCommandSet>()));

            // when
            _tripleStore.Commit(new[] { testUpdates });

            // then
            _strategy.Verify(instance => instance.ExecuteCommandSet(It.Is<SparqlUpdateCommandSet>(set => set.Commands.Single() == testCommand)));
        }

        public class TestCommand : SparqlUpdateCommand
        {
            public TestCommand() : base(SparqlUpdateCommandType.Add)
            {
            }

            public override bool AffectsSingleGraph { get { throw new NotImplementedException(); } }

            public override bool AffectsGraph(Uri graphUri)
            {
                throw new NotImplementedException();
            }

            public override void Evaluate(SparqlUpdateEvaluationContext context)
            {
                throw new NotImplementedException();
            }

            public override void Process(ISparqlUpdateProcessor processor)
            {
                throw new NotImplementedException();
            }

            public override string ToString()
            {
                return "test-command";
            }
        }

        public class TestUpdate : DatasetChange
        {
            public TestUpdate() : base("urn:test:entity", "urn:test:graph")
            {
            }

            public override IDatasetChange MergeWith(IDatasetChange other)
            {
                throw new NotImplementedException();
            }
        }
    }
}