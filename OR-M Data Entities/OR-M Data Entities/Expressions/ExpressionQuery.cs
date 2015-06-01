using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution;
using OR_M_Data_Entities.Expressions.Resolution.Base;

namespace OR_M_Data_Entities.Expressions
{
    public abstract class ExpressionQuery<T> : DbQuery<T>, IEnumerable<T>, IExpressionQuery
    {
        #region Fields
        protected readonly DatabaseReading Context;
        #endregion

        #region Properties
        private List<T> _queryResult { get; set; }

        public bool IsLazyLoadEnabled
        {
            get { return Context == null || Context.IsLazyLoadEnabled; }
        }

        public bool IsSubQuery { get; private set; }
        #endregion

        #region Constructor
        protected ExpressionQuery(DatabaseReading context = null)
            : base(context == null ? QueryInitializerType.None : context.IsLazyLoadEnabled ? QueryInitializerType.None : QueryInitializerType.WithForeignKeys)
        {
            Context = context;
            IsSubQuery = context == null;
        }

        protected ExpressionQuery(IExpressionQueryResolvable query)
            : base(query)
        {
            Context =
                query.GetType().GetField("Context", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(query) as
                    DatabaseReading;
            IsSubQuery = Context == null;
        }
        #endregion

        #region Enumeration Methods
        public IEnumerator<T> GetEnumerator()
        {
            if (IsSubQuery)
            {
                this.GetType().GetMethod("ResolveExpression", BindingFlags.Public | BindingFlags.Instance).Invoke(this, null); 
                return null;
            }

            if (_queryResult != null) return _queryResult.GetEnumerator();

            _queryResult = new List<T>();

            _queryResult = Context.ExecuteQuery(this).ToList();

            return _queryResult.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
