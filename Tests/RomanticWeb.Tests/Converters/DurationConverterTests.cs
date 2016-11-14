using System;
using System.Collections;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RomanticWeb.Converters;
using RomanticWeb.Model;
using RomanticWeb.Vocabularies;

namespace RomanticWeb.Tests.Converters
{
    [TestFixture]
    public class DurationConverterTests : XsdConverterTestsBase<DurationConverter>
    {
        private static IEnumerable TimeSpanValues
        {
            get
            {
                return new object[]
                           {
                               new object[] { "PT1H30M", new Duration(1, 30, 0, 0) },
                               new object[] { "-PT1H30M", new Duration(-1, 30, 0, 0) }
                           };
            }
        }

        [Test]
        [TestCaseSource(typeof(DurationConverterTests), "TimeSpanValues")]
        public void Should_convert_values(string literal, Duration expected)
        {
            var duration = Converter.Convert(Node.ForLiteral(literal), new Mock<IEntityContext>().Object);
            Assert.That(duration, Is.EqualTo(expected));
        }

        [TestCase(Xsd.BaseUri + "duration", typeof(Duration))]
        public void Should_support_converting_supported_xsd_types(string type, Type netType)
        {
            Converter.CanConvert(Node.ForLiteral(string.Empty, new Uri(type))).DatatypeMatches.Should().Be(MatchResult.ExactMatch);
        }
    }
}