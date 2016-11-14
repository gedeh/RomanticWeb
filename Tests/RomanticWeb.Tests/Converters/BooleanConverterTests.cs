using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RomanticWeb.Converters;
using RomanticWeb.Model;
using RomanticWeb.Vocabularies;

namespace RomanticWeb.Tests.Converters
{
    [TestFixture]
    public class BooleanConverterTests : XsdConverterTestsBase<BooleanConverter>
    {
        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("1", true)]
        [TestCase("0", false)]
        public void Should_convert_valid_booleans(string literal, bool expected)
        {
            Assert.That(Converter.Convert(Node.ForLiteral(literal), new Mock<IEntityContext>().Object), Is.EqualTo(expected));
        }

        [TestCase(Xsd.BaseUri + "boolean", typeof(bool))]
        public void Should_support_converting_supported_xsd_types(string type, Type netType)
        {
            Converter.CanConvert(Node.ForLiteral(string.Empty, new Uri(type))).DatatypeMatches.Should().Be(MatchResult.ExactMatch);
        }
    }
}