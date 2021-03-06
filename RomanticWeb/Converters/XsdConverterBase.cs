using System;
using System.Collections.Generic;
using System.Linq;
using RomanticWeb.Model;

namespace RomanticWeb.Converters
{
    /// <summary>A base class for converting XSD-typed literals.</summary>
    public abstract class XsdConverterBase : LiteralNodeConverter
    {
        /// <summary>Get the XSD datatypes, which this converter supports.</summary>
        protected abstract IEnumerable<Uri> SupportedDataTypes { get; }

        /// <summary>Check if a converter can convert the given XSD datatype.</summary>
        public override LiteralConversionMatch CanConvert(INode literalNode)
        {
            var match = new LiteralConversionMatch
            {
                LiteralFormatMatches = MatchResult.DontCare
            };

            if ((literalNode.IsLiteral) && (SupportedDataTypes.Contains(literalNode.DataType, AbsoluteUriComparer.Default)))
            {
                match.DatatypeMatches = MatchResult.ExactMatch;
            }

            return match;
        }
    }
}