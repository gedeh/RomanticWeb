using System;
using RomanticWeb.Converters;
using RomanticWeb.Model;

namespace RomanticWeb.Tests.IntegrationTests.TestMappings
{
    public class AsStringConverter : INodeConverter
    {
        public object Convert(INode objectNode, IEntityContext context)
        {
            return (objectNode.IsUri ? objectNode.Uri.ToString() : objectNode.IsBlank ? objectNode.BlankNode : objectNode.Literal);
        }

        public INode ConvertBack(object value, IEntityContext context)
        {
            throw new NotImplementedException();
        }
    }
}