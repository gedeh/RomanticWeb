using System.Collections.Generic;
using System.Globalization;
using RomanticWeb.Model;

namespace RomanticWeb.Entities
{
    internal static class EntityProxyHelpers
    {
        internal static IEnumerable<INode> WhereMatchesContextRequirements(this IEnumerable<INode> nodes, IEntityContext context)
        {
            var invariantCandidates = new List<INode>();
            var objects = new List<INode>();
            bool ignoreInvariantNodes = false;
            foreach (var node in nodes)
            {
                if (!node.IsLiteral)
                {
                    objects.Add(node);
                }
                else
                {
                    if (context.CurrentCulture.Equals(CultureInfo.InvariantCulture))
                    {
                        if (node.Language == null)
                        {
                            objects.Add(node);
                        }
                    }
                    else
                    {
                        if (node.Language == null)
                        {
                            invariantCandidates.Add(node);
                        }
                        else if (node.Language == context.CurrentCulture.TwoLetterISOLanguageName)
                        {
                            objects.Add(node);
                            ignoreInvariantNodes = true;
                        }
                    }
                }
            }

            if (!ignoreInvariantNodes)
            {
                objects.AddRange(invariantCandidates);
            }

            return objects;
        } 
    }
}