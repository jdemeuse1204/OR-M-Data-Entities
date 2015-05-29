using System;
using System.Collections;
using System.Collections.Generic;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;

namespace OR_M_Data_Entities.Expressions
{
    public abstract class ExpressionQuery<T> : DbQuery<T>, IEnumerable<T>, IExpressionQuery
    {
        #region Fields
        public readonly DatabaseReading Context;
        #endregion

        #region Properties
        private List<T> _queryResult { get; set; }

        public Type Type
        {
            get { return typeof(T); }
        }

        public bool IsLazyLoadEnabled
        {
            get { return Context != null && Context.IsLazyLoadEnabled; }
        }
        #endregion

        #region Constructor
        protected ExpressionQuery(DatabaseReading context = null)
            : base(context == null ? QueryInitializerType.None : context.IsLazyLoadEnabled ? QueryInitializerType.None : QueryInitializerType.WithForeignKeys)
        {
            Context = context;
        }
        #endregion

        #region Enumeration Methods
        public IEnumerator<T> GetEnumerator()
        {
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
