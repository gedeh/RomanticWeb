using System;
using System.Collections.Generic;
using NullGuard;
using RomanticWeb.Model;
using RomanticWeb.Vocabularies;

namespace RomanticWeb.Converters
{
    /// <summary>Converts xsd:duration to <see cref="TimeSpan"/>.</summary>
    public class DurationConverter : XsdConverterBase
    {
        /// <summary>=Gets Uri of xsd:duration.</summary>
        protected override IEnumerable<Uri> SupportedDataTypes { get { yield return Xsd.Duration; } }

        /// <inheritdoc/>
        public override INode ConvertBack(object value, IEntityContext context)
        {
            return Node.ForLiteral(((Duration)value).ToString(), Xsd.Duration);
        }

        /// <inheritdoc />
        [return: AllowNull]
        public override Uri CanConvertBack(Type type)
        {
            return (type == typeof(Duration) ? Xsd.Duration : null);
        }

        /// <inheritdoc/>
        protected override object ConvertInternal(INode literalNode)
        {
            return Duration.Parse(literalNode.Literal);
        }
    }
}