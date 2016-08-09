namespace RomanticWeb.Linq.Model
{
    /// <summary>Provides an abstract for query element.</summary>
    public interface IQueryElement : IQueryComponent
    {
        IQuery OwnerQuery { get; }
    }
}