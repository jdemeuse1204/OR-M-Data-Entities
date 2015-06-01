using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Collections;
using OR_M_Data_Entities.Expressions.Resolution.Join;
using OR_M_Data_Entities.Expressions.Resolution.Select;
using OR_M_Data_Entities.Expressions.Resolution.Select.Info;
using OR_M_Data_Entities.Expressions.Resolution.Where;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class ExpressionQueryResolvable<T> : ExpressionQuery<T>, IExpressionQueryResolvable
    {
        #region Properties
        public IReadOnlyList<SqlDbParameter> Parameters {
            get { return _parameters; }
        }

        private readonly List<SqlDbParameter> _parameters;

        public bool IsLazyLoading { get { return Context != null && Context.IsLazyLoadEnabled; } }

        public IEnumerable<SelectInfo> SelectInfos
        {
            get { return this.SelectList.Infos; }
        }
        #endregion

        #region Constructor
        public ExpressionQueryResolvable()
            : base()
        {
            _parameters = new List<SqlDbParameter>();
        }
        
        public ExpressionQueryResolvable(DatabaseReading context)
            : base(context)
        {
            _parameters = new List<SqlDbParameter>();
        }

        public ExpressionQueryResolvable(IExpressionQueryResolvable query)
            : base(query)
        {
            _parameters =
                query.GetType()
                    .GetField("_parameters", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(query) as List<SqlDbParameter>;
        }
        #endregion

        public void ResolveWhere(Expression<Func<T, bool>> expression)
        {
            WhereExpressionResolver.Resolve(expression, this.WhereResolution, this.Tables);

            _parameters.AddRange(this.WhereResolution.GetParameters());
        }

        public ExpressionQuery<TResult> ResolveJoin<TInner, TKey, TResult>(
            ExpressionQuery<TInner> inner,
            Expression<Func<T, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<T, TInner, TResult>> resultSelector, JoinType joinType)
        {
            JoinExpressionResolver.Resolve(this, 
                inner, 
                outerKeySelector, 
                innerKeySelector,
                resultSelector,
                joinType, 
                this.JoinResolution,
                this.Tables as TableTypeCollection);

            SelectExpressionResolver.Resolve(resultSelector, this.SelectList, this.Tables);

            return new ExpressionQueryResolvable<TResult>(this);
        }

        public ExpressionQuery<TResult> ResolveSelect<TSource,TOuter, TInner, TResult>(Expression<Func<TOuter, TInner, TResult>> selector, ExpressionQuery<TSource> source)
        {
            // What if we are reshaping a reshaped container?

            // need to unselect all for reshape
            SelectList.UnSelectAll();

            // resolve the expressions shape
            SelectExpressionResolver.Resolve(selector, this.SelectList, this.Tables);

            return new ExpressionQueryResolvable<TResult>(this);
        }

        public ExpressionQuery<TResult> ResolveSelect<TSource, TResult>(Expression<Func<TSource, TResult>> selector, ExpressionQuery<TSource> source)
        {
            SelectList.UnSelectAll();

            SelectExpressionResolver.Resolve(selector, this.SelectList, this.Tables);

            return new ExpressionQueryResolvable<TResult>(this);
        }

        public void ResolveTakeRows(int rows)
        {
            
        }

        public void ResolveDistinct()
        {
            
        }

        public void ResolveExpression()
        {
            ResolveQuery();
        } 
    }
}
