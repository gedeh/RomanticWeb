using System;

namespace RomanticWeb.Linq.Model
{
    /// <summary>Expresses a prefix in the query.</summary>
    public interface IPrefix : IQueryElement
    {
        /// <summary>Gets or sets a namespace prefix.</summary>
        string NamespacePrefix { get; set; }

        /// <summary>Gets or sets a namespace URI.</summary>
        Uri NamespaceUri { get; set; }
    }
}