using NUnit.Framework;
using RomanticWeb.Converters;

namespace RomanticWeb.Tests.Converters
{
    [TestFixture]
    public abstract class XsdConverterTestsBase<TConverter> where TConverter : LiteralNodeConverter, new()
    {
        protected TConverter Converter { get; private set; }

        [SetUp]
        public void Setup()
        {
            Converter = new TConverter();
        }
    }
}