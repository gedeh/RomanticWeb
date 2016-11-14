using System;
using System.Collections;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RomanticWeb.Converters;
using RomanticWeb.Model;
using RomanticWeb.Tests.Helpers;
using RomanticWeb.Vocabularies;

namespace RomanticWeb.Tests.Converters
{
    [TestFixture]
    public class DecimalConverterTests : XsdConverterTestsBase<DecimalConverter>
    {
        [Test, Combinatorial]
        public void Should_convert_values_from_literals(
            [ValueSource("LiteralsToConvert")]Tuple<string, decimal> pair,
            [Values("en", "pl")] string culture)
        {
            using (new CultureScope(culture))
            {
                // when
                var value = Converter.Convert(Node.ForLiteral(pair.Item1), new Mock<IEntityContext>().Object);

                // then
                Assert.That(value, Is.InstanceOf<decimal>());
                Assert.That(value, Is.EqualTo(pair.Item2));
            }
        }

        [Test]
        public void Should_not_convert_scientific_notation()
        {
            Converter.Invoking(instance => instance.Convert(Node.ForLiteral("2e10"), new Mock<IEntityContext>().Object)).ShouldThrow<FormatException>();
        }

        [TestCase("some text")]
        [TestCase("2010-09-05")]
        public void Should_not_convert_non_numbers(string literal)
        {
            Converter.Invoking(instance => instance.Convert(Node.ForLiteral(literal), new Mock<IEntityContext>().Object)).ShouldThrow<FormatException>();
        }

        [TestCase(Xsd.BaseUri + "decimal", typeof(decimal))]
        public void Should_support_converting_supported_xsd_types(string type, Type netType)
        {
            Converter.CanConvert(Node.ForLiteral(string.Empty, new Uri(type))).DatatypeMatches.Should().Be(MatchResult.ExactMatch);
        }

        private static IEnumerable LiteralsToConvert()
        {
            yield return new Tuple<string, decimal>("0", 0);
            yield return new Tuple<string, decimal>("-8", -8);
            yield return new Tuple<string, decimal>("2.12", 2.12m);
            yield return new Tuple<string, decimal>("-30.555", -30.555m);
        } 
    }
}