namespace RomanticWeb.Linq.Model
{
    /// <summary>Provides an abstract for query element.</summary>
    public abstract class QueryComponent : IQueryComponent
    {
        private IQuery _ownerQuery;

        /// <summary>Gets an owning query.</summary>
        public virtual IQuery OwnerQuery
        {
            get
            {
                return _ownerQuery;
            }

            set
            {
                _ownerQuery = value;
            }
        }
    }
}