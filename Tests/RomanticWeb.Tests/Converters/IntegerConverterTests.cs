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
    public class IntegerConverterTests : XsdConverterTestsBase<IntegerConverter>
    {
        [TestCase("0", 0)]
        [TestCase("5", 5)]
        [TestCase("-20", -20)]
        public void Should_convert_values_from_literals(string literal, long expectedValue)
        {
            // when
            var value = Converter.Convert(Node.ForLiteral(literal), new Mock<IEntityContext>().Object);

            // then
            Assert.That(value, Is.InstanceOf<long>());
            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [TestCase("2e10")]
        [TestCase("2.3")]
        public void Should_not_convert_decimal_numbers(string value)
        {
            Converter.Invoking(instance => instance.Convert(Node.ForLiteral(value), new Mock<IEntityContext>().Object)).ShouldThrow<FormatException>();
        }

        [TestCase("some text")]
        [TestCase("2010-09-05")]
        public void Should_not_convert_non_numbers(string literal)
        {
            Converter.Invoking(instance => instance.Convert(Node.ForLiteral(literal), new Mock<IEntityContext>().Object)).ShouldThrow<FormatException>();
        }

        [TestCase(Xsd.BaseUri + "integer", typeof(long))]
        [TestCase(Xsd.BaseUri + "int", typeof(int))]
        [TestCase(Xsd.BaseUri + "long", typeof(long))]
        [TestCase(Xsd.BaseUri + "short", typeof(short))]
        [TestCase(Xsd.BaseUri + "byte", typeof(sbyte))]
        [TestCase(Xsd.BaseUri + "nonNegativeInteger", typeof(long))]
        [TestCase(Xsd.BaseUri + "nonPositiveInteger", typeof(long))]
        [TestCase(Xsd.BaseUri + "unsignedInteger", typeof(long))]
        [TestCase(Xsd.BaseUri + "positiveInteger", typeof(long))]
        [TestCase(Xsd.BaseUri + "unsignedByte", typeof(byte))]
        [TestCase(Xsd.BaseUri + "unsignedInt", typeof(uint))]
        [TestCase(Xsd.BaseUri + "unsignedLong", typeof(ulong))]
        [TestCase(Xsd.BaseUri + "unsignedShort", typeof(ushort))]
        public void Should_support_converting_supported_xsd_types(string type, Type netType)
        {
            Converter.CanConvert(Node.ForLiteral(string.Empty, new Uri(type))).DatatypeMatches.Should().Be(MatchResult.ExactMatch);
        }
    }
}