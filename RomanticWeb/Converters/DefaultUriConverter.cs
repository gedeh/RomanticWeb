﻿using System;
using RomanticWeb.Model;

namespace RomanticWeb.Converters
{
    /// <summary>Generic converter for any type of entity id.</summary>
    public class DefaultUriConverter : INodeConverter
    {
        /// <summary>Converts an uri node to URI.</summary>
        public object Convert(INode objectNode, IEntityContext context)
        {
            if (objectNode.IsBlank)
            {
                throw new ArgumentOutOfRangeException("objectNode", "Cannot convert blank node to URI");
            }

            if (objectNode.IsUri)
            {
                return objectNode.Uri;
            }

            return new Uri(objectNode.Literal);
        }

        /// <inheritdoc />
        public INode ConvertBack(object obj, IEntityContext context)
        {
            return Node.ForUri(((Uri)obj));
        }
    }
}