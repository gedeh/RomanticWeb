using System;
using System.Collections.Generic;

namespace RomanticWeb.Collections
{
    /// <summary>Defines a contract of a component that depends on another component's types.</summary>
    public interface IDependentComponent
    {
        /// <summary>Gets an enumeration of types this component depends on.</summary>
        IEnumerable<Type> Requires { get; }
    }
}
