namespace RomanticWeb.Entities
{
    /// <summary>Contract for generating blank node Ids.</summary>
    public interface IBlankNodeIdGenerator
    {
        /// <summary>Generates a blank node identifier.</summary>
        /// <returns>a new node every time.</returns>
        string Generate();
    }
}