using System;
using System.Globalization;
using RomanticWeb.Model;
using RomanticWeb.Vocabularies;

namespace RomanticWeb.Converters
{
    /// <summary>Converts string literals.</summary>
    public class StringConverter : LiteralNodeConverter
    {
        /// <inheritdoc/>
        public override INode ConvertBack(object value, IEntityContext context)
        {
            return (context.CurrentCulture.Equals(CultureInfo.InvariantCulture) ? 
                Node.ForLiteral(value.ToString(), Xsd.String) : 
                Node.ForLiteral(value.ToString(), context.CurrentCulture.TwoLetterISOLanguageName));
        }

        /// <inheritdoc/>
        public override LiteralConversionMatch CanConvert(INode literalNode)
        {
            var literalConversionMatch = new LiteralConversionMatch { LiteralFormatMatches = MatchResult.DontCare };
            if ((literalNode.IsLiteral) && ((literalNode.DataType == null) || (AbsoluteUriComparer.Default.Compare(literalNode.DataType, Xsd.String) == 0)))
            {
                literalConversionMatch.DatatypeMatches = MatchResult.ExactMatch;
            }

            return literalConversionMatch;
        }

        /// <inheritdoc/>
        public override Uri CanConvertBack(Type type)
        {
            return (type == typeof(string) ? Xsd.String : null);
        }

        /// <inheritdoc/>
        protected override object ConvertInternal(INode objectNode)
        {
            if (!objectNode.IsLiteral)
            {
                return objectNode.ToString();
            }

            return objectNode.Literal;
        }
    }
}