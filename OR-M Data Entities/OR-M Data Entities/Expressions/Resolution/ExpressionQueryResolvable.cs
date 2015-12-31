/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Collections;
using OR_M_Data_Entities.Expressions.Query.Columns;
using OR_M_Data_Entities.Expressions.Query.Tables;
using OR_M_Data_Entities.Expressions.Resolution.Join;
using OR_M_Data_Entities.Expressions.Resolution.Order;
using OR_M_Data_Entities.Expressions.Resolution.Select;
using OR_M_Data_Entities.Expressions.Resolution.Where;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class ExpressionQueryResolvable<T> : OrderedExpressionQuery<T>, IExpressionQueryResolvable
    {
        #region Properties
        public IReadOnlyList<SqlDbParameter> Parameters
        {
            get { return WhereResolution.Parameters; }
        }

        private readonly List<SqlDbParameter> _parameters;

        public OSchematic LoadSchematic
        {
            get { return QuerySchematic; }
        }

        public void Initialize()
        {
            this.InitializeQuery();
        }

        public int GetOrdinalBySelectedColumns(int oldOrdinal)
        {
            return SelectInfos.Count(w => w.Ordinal <= oldOrdinal && w.IsSelected) - 1;
        }

        public DatabaseReading DbContext {
            get { return this.Context; }
        }

        public IEnumerable<DbColumn> SelectInfos
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
        public ExpressionQueryResolvable(DatabaseReading context, string viewId = null)
            : base(context, viewId)
        {
            _parameters = new List<SqlDbParameter>();
        }

        public ExpressionQueryResolvable(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
            : base(query, constructionType)
        {
            _parameters =
                (query is IExpressionQueryViewResolvable ? query.GetType().BaseType : query.GetType())
                    .GetField("_parameters", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(query) as List<SqlDbParameter>;
        }
        #endregion

        #region Methods
        public OrderedExpressionQuery<T> ResolveOrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            OrderExpressionResolver.ResolveDescending(keySelector, this.Columns);

            return new ExpressionQueryResolvable<T>(this, ExpressionQueryConstructionType.Order);
        }

        public OrderedExpressionQuery<T> ResolveOrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            OrderExpressionResolver.Resolve(keySelector, this.Columns);

            return new ExpressionQueryResolvable<T>(this, ExpressionQueryConstructionType.Order);
        }

        public void ResolveWhere(Expression<Func<T, bool>> expression)
        {
            WhereExpressionResolver.Resolve(expression, this.WhereResolution, this, _getViewId());
        }

        public void ResolveFind(object[] pks)
        {
            WhereExpressionResolver.ResolveFind<T>(pks, this.WhereResolution, this);
        }

        private string _getViewId()
        {
            return this is IExpressionQueryViewResolvable ? ((IExpressionQueryViewResolvable)this).ViewId : null;
        }

        public ExpressionQuery<TResult> ResolveJoin<TInner, TKey, TResult>(
            ExpressionQuery<TInner> inner,
            Expression<Func<T, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<T, TInner, TResult>> resultSelector, JoinType joinType)
        {
            var tables = (TableCollection)this.Tables;

            // since we are joining, if the table doesnt have any foreign keys the 
            // table will not exist so we need to add it
            if (!tables.ContainsType(typeof(TInner), this.Id))
            {
                // does not come from a fk
                tables.Add(new ForeignKeyTable(this.Id, typeof(TInner), null, null));
            }

            JoinExpressionResolver.Resolve(this,
                inner,
                outerKeySelector,
                innerKeySelector,
                resultSelector,
                joinType,
                this.JoinResolution,
                this.Tables as TableCollection,
                this.Id);

            SelectExpressionResolver.Resolve(resultSelector, this.Columns, this, _getViewId());

            return new ExpressionQueryResolvable<TResult>(this, ExpressionQueryConstructionType.Join);
        }

        public ExpressionQuery<TResult> ResolveSelect<TSource, TOuter, TInner, TResult>(Expression<Func<TOuter, TInner, TResult>> selector, ExpressionQuery<TSource> source)
        {
            // resolve the expressions shape
            SelectExpressionResolver.Resolve(selector, this.Columns, this, _getViewId());

            return new ExpressionQueryResolvable<TResult>(this, ExpressionQueryConstructionType.Select);
        }

        public ExpressionQuery<TResult> ResolveSelect<TSource, TResult>(Expression<Func<TSource, TResult>> selector, ExpressionQuery<TSource> source)
        {
            SelectExpressionResolver.Resolve(selector, this.Columns, this, _getViewId());

            return new ExpressionQueryResolvable<TResult>(this, ExpressionQueryConstructionType.Select);
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

        public void ResoveMax()
        {
            Function = FunctionType.Max;
        }

        public void ResoveMin()
        {
            Function = FunctionType.Min;
        }

        public void ResolveCount()
        {
            Function = FunctionType.Count;
        }

        public void ResolveInclude(string tableName)
        {
            
        }
        #endregion
    }
}
