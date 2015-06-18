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
    public abstract class ExpressionQuery<T> : DbQuery<T>, IEnumerable<T>, IExpressionQuery
    {
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
            if (IsSubQuery)
            {
                // make sure sub query works with yield
                GetType().GetMethod("ResolveExpression", BindingFlags.Public | BindingFlags.Instance).Invoke(this, null);
                yield return default (T);
            }

            foreach (var item in Context.ExecuteQuery(this))
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
