/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution;

namespace OR_M_Data_Entities.Expressions
{
    public abstract class ExpressionQuery<T> : DbQuery<T>, IEnumerator<T>, IExpressionQuery
    {
        #region Fields
        private readonly object _lock = new object();
        #endregion

        #region Constructor
        protected ExpressionQuery(DatabaseReading context, string viewId = null)
            : base(context, viewId)
        {

        }

        protected ExpressionQuery(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
            : base(query, constructionType)
        {
            // for subquery's, joins, and such

            // view cannot be in a sub query
        }
        #endregion

        #region Enumeration Methods
        public IEnumerator<T> GetEnumerator()
        {
            lock (_lock)
            {
                if (IsSubQuery)
                {
                    // make sure sub query works with yield
                    GetType().GetMethod("ResolveExpression", BindingFlags.Public | BindingFlags.Instance).Invoke(this, null);
                    yield return default(T);
                }

                foreach (var item in Context.ExecuteQuery(this))
                {
                    yield return item;
                }

                Context.Dispose();
            }
        }
        #endregion

        public void Dispose()
        {
            Context.Dispose();
        }

        public bool MoveNext()
        {
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }

        public T Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
