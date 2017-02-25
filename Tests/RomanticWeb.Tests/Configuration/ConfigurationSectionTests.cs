using System;
using System.Configuration;
#if NETSTANDARD16
using System.IO;
#endif
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RomanticWeb.Configuration;

namespace RomanticWeb.Tests
{
    [TestFixture]
    public class ConfigurationTests : ConfigurationTest
    {
        private ConfigurationSectionHandler _configuration;

        [SetUp]
        public void Setup()
        {
#if NETSTANDARD16
            Directory.SetCurrentDirectory(Path.Combine(BinDirectory, "netcoreapp1.0"));
#endif
            _configuration = ConfigurationSectionHandler.Default;
        }
#if NETSTANDARD16

        [TearDown]
        public void Teardown()
        {
            Directory.SetCurrentDirectory(BaseDirectory);
        }
#endif

        [Test]
        public void Should_contain_added_assemblies()
        {
            // given
            var factory = _configuration.Factories.Cast<FactoryElement>().First(item => item.Name == "default");

            // then
            factory.MappingAssemblies.Should().HaveCount(2);
            factory.MappingAssemblies.Cast<MappingAssemblyElement>()
                                     .Select(e => e.Assembly)
                                     .Should().ContainInOrder("Magi.Balthazar.Contracts", "Magi.Web");
        }

        [Test]
        public void Should_contain_added_ontology_prefixes()
        {
            // given
            var factory = _configuration.Factories.Cast<FactoryElement>().First(item => item.Name == "default");

            // then
            factory.Ontologies.Should().HaveCount(2);
            factory.Ontologies.Cast<OntologyElement>()
                              .Select(e => e.Prefix)
                              .Should().ContainInOrder("lemon", "frad");
            factory.Ontologies.Cast<OntologyElement>()
                              .Select(e => e.Uri.ToString())
                              .Should().ContainInOrder("http://www.monnet-project.eu/lemon#", "http://iflastandards.info/ns/fr/frad/");
        }

        [Test]
        public void Should_contain_default_base_uri()
        {
            // given
            var factory = _configuration.Factories.Cast<FactoryElement>().First(item => item.Name == "default");

            // then
            factory.BaseUris.Default.Should().Be(new Uri("http://www.romanticweb.com/"));
        }

        [Test]
        public void Should_contain_meta_graph_uri()
        {
            // given
            var factory = _configuration.Factories.Cast<FactoryElement>().First(item => item.Name == "default");

            // then
            factory.MetaGraphUri.Should().Be(new Uri("http://meta.romanticweb.com/"));
        }

        [Test]
        public void Should_require_meta_graph_uri()
        {
            Assert.Throws<ConfigurationErrorsException>(() => ConfigurationSectionHandler.GetConfiguration("missingMetaGraph"));
        }

        [Test]
        public void Empty_configuration_should_be_populated()
        {
            // given
            var factory = _configuration.Factories.Cast<FactoryElement>().First(item => item.Name == "empty");

            // then
            factory.Ontologies.Should().BeEmpty();
            factory.MappingAssemblies.Should().BeEmpty();
            factory.MetaGraphUri.Should().Be(new Uri("http://graphs.example.com"));
            factory.BaseUris.Default.Should().BeNull();
        }
    }
}