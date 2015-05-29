using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Join;
using OR_M_Data_Entities.Expressions.Resolution.Select;
using OR_M_Data_Entities.Expressions.Resolution.Where;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class ExpressionQueryResolvable<T> : ExpressionQuery<T>, IExpressionQueryResolvable
    {
        #region Properties
        public IReadOnlyCollection<SqlDbParameter> Parameters { get; private set; }
        #endregion

        #region Constructor

        public ExpressionQueryResolvable(DatabaseReading context = null)
            : base(context)
        {
            
        }
        #endregion

        public void ResolveWhere(Expression<Func<T, bool>> expression)
        {
            WhereExpressionResolver.Resolve(expression, this.WhereResolution);
        }
        public ExpressionQuery<TResult> ResolveInnerJoin<TInner, TKey, TResult>(
            ExpressionQuery<TInner> inner,
            Expression<Func<T, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<T, TInner, TResult>> resultSelector)
        {
            JoinExpressionResolver.Resolve(this, inner, outerKeySelector, innerKeySelector,
                resultSelector,
                null, JoinType.Left, this.JoinResolution);

            return null;
        }

        public ExpressionQuery<TResult> ResolveSelect<TSource,TOuter, TInner, TResult>(Expression<Func<TOuter, TInner, TResult>> selector, ExpressionQuery<TSource> source)
        {
            // What if we are reshaping a reshaped container?

            // need to unselect all for reshape
            SelectList.UnSelectAll();

            // resolve the expressions shape
            SelectExpressionResolver.Resolve(selector, this.SelectList, this.Tables);

            return null;
        }

        public ExpressionQuery<TResult> ResolveSelect<TSource, TResult>(Expression<Func<TSource, TResult>> selector, ExpressionQuery<TSource> source)
        {
            SelectList.UnSelectAll();

            SelectExpressionResolver.Resolve(selector, this.SelectList, this.Tables);

            return null;
        }

        public void ResolveTakeRows(int rows)
        {
            
        }

        public void ResolveDistinct()
        {
            
        }

        public void ResolveExpression()
        {
            throw new NotImplementedException();
        } 
    }
}
