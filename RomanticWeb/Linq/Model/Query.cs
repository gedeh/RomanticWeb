using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using RomanticWeb.Linq.Model.Navigators;

namespace RomanticWeb.Linq.Model
{
    /// <summary>Represents a whole query.</summary>
    [QueryComponentNavigator(typeof(QueryNavigator))]
    public class Query : QueryElement, IExpression, IQuery
    {
        #region Fieds
        private readonly IList<IPrefix> _prefixes;
        private readonly IList<ISelectableQueryComponent> _select;
        private readonly IList<IQueryElement> _elements;
        private readonly IVariableNamingStrategy _variableNamingStrategy;
        private readonly IVariableNamingConvention _variableNamingConvention;
        private readonly IDictionary<IExpression, bool> _orderBy = new Dictionary<IExpression, bool>();
        private Identifier _subject;
        private QueryForms _queryForm;
        private int _offset = -1;
        private int _limit = -1;
        #endregion

        #region Constructors
        /// <summary>Constructor with subject and variable naming strategy passed.</summary>
        /// <param name="variableNamingStrategy">Varialbe naming strategy.</param>
        /// <param name="variableNamingConvention">Variable naming convention.</param>
        internal Query(IVariableNamingStrategy variableNamingStrategy, IVariableNamingConvention variableNamingConvention)
        {
            _variableNamingStrategy = variableNamingStrategy;
            _variableNamingConvention = variableNamingConvention;
            _queryForm = QueryForms.Select;
            ObservableCollection<IPrefix> prefixes = new ObservableCollection<IPrefix>();
            prefixes.CollectionChanged += OnCollectionChanged;
            _prefixes = prefixes;
            ObservableCollection<ISelectableQueryComponent> select = new ObservableCollection<ISelectableQueryComponent>();
            select.CollectionChanged += OnCollectionChanged;
            _select = select;
            ObservableCollection<IQueryElement> elements = new ObservableCollection<IQueryElement>();
            elements.CollectionChanged += OnCollectionChanged;
            _elements = elements;
        }

        /// <summary>Constructor with subject and variable naming strategy passed.</summary>
        /// <param name="subject">Subject of this query.</param>
        /// <param name="variableNamingStrategy">Varialbe naming strategy.</param>
        /// <param name="variableNamingConvention">Variable naming convention.</param>
        internal Query(Identifier subject, IVariableNamingStrategy variableNamingStrategy, IVariableNamingConvention variableNamingConvention) : this(variableNamingStrategy, variableNamingConvention)
        {
            if ((_subject = subject) != null)
            {
                _subject.OwnerQuery = this;
            }
        }
        #endregion

        #region Properties
        /// <summary>Gets an enumeration of all prefixes.</summary>
        public IList<IPrefix> Prefixes { get { return _prefixes; } }

        /// <summary>Gets an enumeration of all selected expressions.</summary>
        public IList<ISelectableQueryComponent> Select { get { return _select; } }

        /// <summary>Gets an enumeration of all query elements.</summary>
        public IList<IQueryElement> Elements { get { return _elements; } }

        /// <summary>Gets a value indicating if the given query is actually a sub query.</summary>
        public bool IsSubQuery { get { return (OwnerQuery != null); } }

        /// <summary>Gets a query form of given query.</summary>
        public QueryForms QueryForm { get { return _queryForm; } internal set { _queryForm = value; } }

        /// <summary>Gets or sets the offset.</summary>
        public int Offset { get { return _offset; } set { _offset = (value >= 0 ? value : -1); } }

        /// <summary>Gets or sets the limit.</summary>
        public int Limit { get { return _limit; } set { _limit = (value >= 0 ? value : -1); } }

        /// <summary>Gets a map of order by clauses.</summary>
        /// <remarks>Key is the expression on which the sorting should be performed and the value determines the direction, where <b>true</b> means descending and <b>false</b> is for ascending (default).</remarks>
        public IDictionary<IExpression, bool> OrderBy { get { return _orderBy; } }

        /// <summary>Gets an owning query.</summary>
        public override IQuery OwnerQuery
        {
            get
            {
                return base.OwnerQuery;
            }

            set
            {
                if (value != null)
                {
                    base.OwnerQuery = value;
                }
            }
        }

        /// <summary>Subject of this query.</summary>
        internal Identifier Subject
        {
            get
            {
                return _subject;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("subject");
                }

                (_subject = value).OwnerQuery = this;
            }
        }
        #endregion

        #region Public methods
        /// <summary>Creates a new blank query that can act as a sub query for this instance.</summary>
        /// <param name="subject">Primary subject of the resulting query.</param>
        /// <remarks>This method doesn't add the resulting query as a sub query of this instance.</remarks>
        /// <returns>Query that can act as a sub query for this instance.</returns>
        public Query CreateSubQuery(Identifier subject)
        {
            Query result = new Query(subject, _variableNamingStrategy, _variableNamingConvention);
            result.OwnerQuery = this;
            return result;
        }

        /// <summary>Creates a variable name from given identifier.</summary>
        /// <param name="identifier">Identifier to be used to abbreviate variable name.</param>
        /// <returns>Variable name with unique name.</returns>
        public string CreateVariableName(string identifier)
        {
            return _variableNamingStrategy.GetNameForIdentifier(this, CreateIdentifier(identifier));
        }

        /// <summary>Retrieves an identifier from a passed variable name.</summary>
        /// <param name="variableName">Variable name to retrieve identifier from.</param>
        /// <returns>Identifier passed to create the variable name.</returns>
        public string RetrieveIdentifier(string variableName)
        {
            return _variableNamingStrategy.ResolveNameToIdentifier(variableName);
        }

        /// <summary>Creates an identifier from given name.</summary>
        /// <param name="name">Name.</param>
        /// <returns>Identifier created from given name.</returns>
        public string CreateIdentifier(string name)
        {
            return _variableNamingConvention.GetIdentifierForName(name);
        }

        /// <summary>Creates a string representation of this query.</summary>
        /// <returns>String representation of this query.</returns>
        public override string ToString()
        {
            return System.String.Format(
                "{3} SELECT {1} {0}WHERE {0}{{{0}{2}{0}}}",
                Environment.NewLine,
                System.String.Join(" ", _select.Select(item => (item is StrongEntityAccessor ? System.String.Format("?G{0} ?{0}", ((StrongEntityAccessor)item).About.Name) : item.ToString()))),
                System.String.Join(Environment.NewLine, _elements),
                System.String.Join(Environment.NewLine, _prefixes));
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="operand">Type: <see cref="System.Object" />
        /// The object to compare with the current object.</param>
        /// <returns>Type: <see cref="System.Boolean" />
        /// <b>true</b> if the specified object is equal to the current object; otherwise, <b>false</b>.</returns>
        public override bool Equals(object operand)
        {
            return (!Object.Equals(operand, null)) && (operand.GetType() == typeof(Query)) && (_queryForm == ((Query)operand)._queryForm) &&
                (_subject != null ? _subject.Equals(((Query)operand)._subject) : Object.Equals(((Query)operand)._subject, null)) &&
                (_variableNamingStrategy == ((Query)operand)._variableNamingStrategy) &&
                (_prefixes.Count == ((Query)operand)._prefixes.Count) &&
                (_select.Count == ((Query)operand)._select.Count) &&
                (_elements.Count == ((Query)operand)._elements.Count) &&
                (_prefixes.Count == ((Query)operand)._prefixes.Count) &&
                (_prefixes.SequenceEqual(((Query)operand)._prefixes)) &&
                (_select.SequenceEqual(((Query)operand)._select)) &&
                (_elements.SequenceEqual(((Query)operand)._elements));
        }

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>Type: <see cref="System.Int32" />
        /// A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return typeof(Query).FullName.GetHashCode() ^ _variableNamingStrategy.GetHashCode() ^ _queryForm.GetHashCode();
        }
        #endregion

        #region Non-public methods
        /// <summary>Rised when arguments collection has changed.</summary>
        /// <param name="sender">Sender of this event.</param>
        /// <param name="e">Eventarguments.</param>
        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (QueryComponent queryComponent in e.NewItems)
                        {
                            if (queryComponent != null)
                            {
                                queryComponent.OwnerQuery = this;
                            }
                        }

                        break;
                    }
            }
        }
        #endregion
    }
}