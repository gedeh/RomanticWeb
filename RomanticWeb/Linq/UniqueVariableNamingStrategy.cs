using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RomanticWeb.Linq.Model;

namespace RomanticWeb.Linq
{
    /// <summary>Provides constistent and non-coliding names for identifiers.</summary>
    internal class UniqueVariableNamingStrategy : IVariableNamingStrategy
    {
        private static readonly IDictionary<int, IDictionary<string, int>> NextIdentifiers = new ConcurrentDictionary<int, IDictionary<string, int>>();

        /// <summary>Gets a variable name for given identifier.</summary>
        /// <param name="queryContext">Query context.</param>
        /// <param name="identifier">Base identifier for which the name must be created.</param>
        /// <returns>Name of the variale coresponding for given identifier.</returns>
        public string GetNameForIdentifier(IQuery queryContext, string identifier)
        {
            if (identifier.Length == 0)
            {
                throw new InvalidOperationException("Cannot create a variable name for empty identifier.");
            }

            if (Char.IsNumber(identifier[identifier.Length - 1]))
            {
                identifier += '_';
            }

            string result = identifier;
            if (!NextIdentifiers.ContainsKey(queryContext.GetHashCode()))
            {
                NextIdentifiers[queryContext.GetHashCode()] = new ConcurrentDictionary<string, int>();
            }

            if (!NextIdentifiers[queryContext.GetHashCode()].ContainsKey(identifier))
            {
                NextIdentifiers[queryContext.GetHashCode()][identifier] = 0;
            }

            result += (NextIdentifiers[queryContext.GetHashCode()][identifier]++).ToString();

            return result;
        }

        /// <summary>Reverses the process and resolves an initial identifier passed to create a variable name.</summary>
        /// <param name="name">Name to be resolved.</param>
        /// <returns>Identifier that was passed to create a given variable name.</returns>
        public string ResolveNameToIdentifier(string name)
        {
            if (name.Length == 0)
            {
                throw new InvalidOperationException("Cannot resolve an identifier for empty variable name.");
            }

            string result = name.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            if ((result.Length > 1) && (result[result.Length - 1] == '_') && (Char.IsNumber(result[result.Length - 2])))
            {
                return result.TrimEnd('_');
            }

            return result;
        }
    }
}