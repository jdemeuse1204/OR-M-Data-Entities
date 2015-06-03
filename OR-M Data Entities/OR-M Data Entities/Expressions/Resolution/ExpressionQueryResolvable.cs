using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Collections;
using OR_M_Data_Entities.Expressions.Query;
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
            get { return WhereResolution.Parameters; }
        }

        private readonly List<SqlDbParameter> _parameters;

        public bool IsLazyLoading { get { return Context != null && Context.IsLazyLoadEnabled; } }

        public void Initialize()
        {
            this.InitializeSelectInfos();
        }

        public IEnumerable<SelectInfo> SelectInfos
        {
            get { return this.Columns.Infos; }
        }

        public void Clear()
        {
            this.ClearJoinQuery();
            this.ClearSelectQuery();
            this.ClearWhereQuery();
        }

        #endregion

        #region Constructor        
        public ExpressionQueryResolvable(DatabaseReading context)
            : base(context)
        {
            _parameters = new List<SqlDbParameter>();
        }

        public ExpressionQueryResolvable(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
            : base(query, constructionType)
        {
            _parameters =
                query.GetType()
                    .GetField("_parameters", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(query) as List<SqlDbParameter>;
        }
        #endregion

        #region Methods
        public void ResolveWhere(Expression<Func<T, bool>> expression)
        {
            WhereExpressionResolver.Resolve(expression, this.WhereResolution, this);
        }

        public ExpressionQuery<TResult> ResolveJoin<TInner, TKey, TResult>(
            ExpressionQuery<TInner> inner,
            Expression<Func<T, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<T, TInner, TResult>> resultSelector, JoinType joinType)
        {
            var tables = (TableTypeCollection)this.Tables;

            // since we are joining, if the table doesnt have any foreign keys the 
            // table will not exist so we need to add it
            if (!tables.ContainsType(typeof (TInner), this.Id))
            {
                tables.Add(new PartialTableType(typeof(TInner), this.Id, null));
            }

            JoinExpressionResolver.Resolve(this, 
                inner, 
                outerKeySelector, 
                innerKeySelector,
                resultSelector,
                joinType, 
                this.JoinResolution,
                this.Tables as TableTypeCollection,
                this.Id);

            SelectExpressionResolver.Resolve(resultSelector, this.Columns, this);

            return new ExpressionQueryResolvable<TResult>(this, ExpressionQueryConstructionType.Join);
        }

        public ExpressionQuery<TResult> ResolveSelect<TSource,TOuter, TInner, TResult>(Expression<Func<TOuter, TInner, TResult>> selector, ExpressionQuery<TSource> source)
        {
            // resolve the expressions shape
            SelectExpressionResolver.Resolve(selector, this.Columns, this);

            return new ExpressionQueryResolvable<TResult>(this, ExpressionQueryConstructionType.Main);
        }

        public ExpressionQuery<TResult> ResolveSelect<TSource, TResult>(Expression<Func<TSource, TResult>> selector, ExpressionQuery<TSource> source)
        {
            SelectExpressionResolver.Resolve(selector, this.Columns, this);

            return new ExpressionQueryResolvable<TResult>(this, ExpressionQueryConstructionType.Main);
        }

        public void ResolveTakeRows(int rows)
        {
            this.Columns.TakeRows = rows;
        }

        public void ResolveDistinct()
        {
            this.Columns.IsSelectDistinct = true;
        }

        public void ResolveExpression()
        {
            ResolveQuery();
        }
        #endregion
    }
}
