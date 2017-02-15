using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RomanticWeb.DotNetRDF;
using RomanticWeb.Vocabularies;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace RomanticWeb.Tests.DotNetRDF
{
    [TestFixture]
    public class FileTripleStoreTests
    {
        private const string Subject = "http://magi/addresses/Address";
        private const string Predicate = "http://schema.org/addressLocality";
        private const string Value = "Łódź";
        private const string EntityName = "test://entity/";
        private string filePath = null;

        [SetUp]
        protected void Setup()
        {
            var binDirectory = AppDomainExtensions.GetPrimaryAssemblyPath();
            filePath = Path.Combine(binDirectory, "test.trig");
            if (!Directory.Exists(binDirectory))
            {
                Directory.CreateDirectory(binDirectory);
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            FileStream fileStream = File.Create(filePath);
            GetType().GetTypeInfo().Assembly.GetManifestResourceStream("RomanticWeb.Tests.TestGraphs.TriplesWithLiteralSubjects.trig").CopyTo(fileStream);
            fileStream.Close();
        }

        [Test]
        public void Should_use_provided_data_as_triple_store()
        {
            // Given
            MemoryStream stream = CreateMemoryStream();

            // When
            FileTripleStore tripleStore = new FileTripleStore(stream, new TriGParser(), new TriGWriter());

            // Then
            AssertTriplesCount(tripleStore);
        }

        [Test]
        public void Should_write_inserted_statements_to_stream()
        {
            // Given
            MemoryStream stream = CreateMemoryStream();
            FileTripleStore tripleStore = new FileTripleStore(stream, new TriGParser(), new TriGWriter());

            // When
            InsertTriple(tripleStore);

            // Then
            AssertTripleExists(tripleStore);
            AssertTripleExists(System.Text.UTF8Encoding.UTF8.GetString(stream.ToArray()), true);
        }

        [Test]
        public void Should_write_deleted_statements_to_stream()
        {
            // Given
            MemoryStream stream = CreateMemoryStream();
            FileTripleStore tripleStore = new FileTripleStore(stream, new TriGParser(), new TriGWriter());

            // When
            DeleteTriple(tripleStore);

            // Then
            AssertTripleNotExists(tripleStore);
            AssertEntityNotExists(System.Text.UTF8Encoding.UTF8.GetString(stream.ToArray()));
        }

        [Test]
        public void Should_use_provided_file_as_triple_store()
        {
            // Given

            // When
            FileTripleStore tripleStore = new FileTripleStore(filePath);

            // Then
            AssertTriplesCount(tripleStore);
        }

        [Test]
        public void Should_write_inserted_statements_to_file()
        {
            // Given
            FileTripleStore tripleStore = new FileTripleStore(filePath);

            // When
            InsertTriple(tripleStore);

            // Then
            AssertTripleExists(tripleStore);
            AssertTripleExists(File.ReadAllText(filePath));
        }

        [Test]
        public void Should_write_deleted_statements_to_file()
        {
            // Given
            FileTripleStore tripleStore = new FileTripleStore(filePath);

            // When
            DeleteTriple(tripleStore);

            // Then
            AssertTripleNotExists(tripleStore);
            AssertEntityNotExists(File.ReadAllText(filePath));
        }

        [Test]
        public void Should_handle_concurrent_operations()
        {
            // Given
            int maxUsers = 4;
            FileTripleStore tripleStore = new FileTripleStore(filePath);

            // When
            CountdownEvent synchronizationContext = new CountdownEvent(maxUsers);
            Task.Factory.StartNew(() => { InsertTriple(tripleStore, 0); synchronizationContext.Signal(); });
            Task.Factory.StartNew(() => { InsertTriple(tripleStore, 1); synchronizationContext.Signal(); });
            Task.Factory.StartNew(() => { InsertTriple(tripleStore, 2); synchronizationContext.Signal(); });
            Task.Factory.StartNew(() => { InsertTriple(tripleStore, 3); synchronizationContext.Signal(); });
            synchronizationContext.Wait();

            // Then
            for (int count = 0; count < maxUsers; count++)
            {
                AssertTripleExists(tripleStore, count);
            }
        }

        private MemoryStream CreateMemoryStream()
        {
            MemoryStream stream = new MemoryStream();
            GetType().GetTypeInfo().Assembly.GetManifestResourceStream("RomanticWeb.Tests.TestGraphs.TriplesWithLiteralSubjects.trig").CopyTo(stream);
            return stream;
        }

        private void AssertTriplesCount(ITripleStore tripleStore)
        {
            tripleStore.Triples.Count().Should().BeGreaterThan(0);
        }

        private void InsertTriple(IUpdateableTripleStore tripleStore, int discriminator = 0)
        {
            tripleStore.ExecuteUpdate(System.String.Format("INSERT DATA {{ <{0}> a <{1}> . }}", EntityName + (discriminator == 0 ? System.String.Empty : discriminator.ToString()), Ldp.Container));
        }

        private void AssertTripleExists(ITripleStore tripleStore, int discriminator = 0)
        {
            tripleStore.Triples.Count(item => (item.Subject is IUriNode) && (((IUriNode)item.Subject).Uri.ToString() == EntityName + (discriminator == 0 ? System.String.Empty : discriminator.ToString()))).Should().Be(1);
        }

        private void AssertTripleExists(string input, bool withoutCompression = false)
        {
            var statement = System.String.Format(
                "\\<{0}\\>[ \t\n\r]+{1}[ \t\n\r]+\\<{2}\\>",
                EntityName,
                (withoutCompression ? "a" : String.Format("\\<{0}\\>", Rdf.type)),
                Ldp.Container);
            input.Should().MatchRegex(statement);
        }

        private void DeleteTriple(IUpdateableTripleStore tripleStore)
        {
            tripleStore.ExecuteUpdate(System.String.Format("DELETE DATA {{ GRAPH <http://data.magi/addresses/Address> {{ <{0}> <{1}> \"{2}\" }} }}", Subject, Predicate, Value));
        }

        private void AssertTripleNotExists(ITripleStore tripleStore)
        {
            tripleStore.Triples.Count(item =>
                (item.Subject is IUriNode) && (((IUriNode)item.Subject).Uri.ToString() == Subject) &&
                (((IUriNode)item.Predicate).Uri.ToString() == Predicate) &&
                (item.Object is ILiteralNode) && (((ILiteralNode)item.Object).ToString() == Value)).Should().Be(0);
        }

        private void AssertEntityNotExists(string input)
        {
            input.Should().NotMatchRegex(System.String.Format("\\<{0}\\>[ \t\n\r]+\\<{1}\\>[ \t\n\r]+\"{2}\"", Subject, Predicate, Value));
        }
    }
}