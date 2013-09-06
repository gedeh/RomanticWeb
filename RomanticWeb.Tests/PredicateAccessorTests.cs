﻿using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using VDS.RDF;

namespace RomanticWeb.Tests
{
    [TestFixture]
    public class PredicateAccessorTests
    {
        private readonly EntityId _entityId = new EntityId(Uri);
        private Mock<ITripleStore> _store;
        private Ontology _ontology;
        private const string Uri = "urn:test:identity";

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            _ontology = new Stubs.StaticOntologyProvider().Ontologies.First();
        }

        [SetUp]
        public void Setup()
        {
            _store = new Mock<ITripleStore>(MockBehavior.Strict);
        }

        [Test]
        public void Getting_known_predicate_should_return_subjects()
        {
            // given
            var graph = new Mock<IGraph>(MockBehavior.Strict);
            graph.Setup(g => g.GetTriplesWithSubjectPredicate(It.IsAny<IUriNode>(), It.IsAny<IUriNode>())).Returns(new Triple[0]);
            _store.Setup(s => s.Graphs[null]).Returns(graph.Object);
            dynamic accessor = new dotNetRDF.PredicateAccessor(_store.Object, _entityId, _ontology);

            // when
            var givenName = accessor.givenName;

            // then
            graph.Verify(s => s.GetTriplesWithSubjectPredicate(It.Is<IUriNode>(n => n.Uri.Equals(new Uri(Uri))),
                                                               It.Is<IUriNode>(n => n.Uri.Equals(new Uri("http://xmlns.com/foaf/0.1/givenName")))),
                                                               Times.Once);
        }
    }
}