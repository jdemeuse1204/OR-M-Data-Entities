/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.LambdaResolution;
using OR_M_Data_Entities.Expressions.ObjectMapping;
using OR_M_Data_Entities.Expressions.ObjectMapping.Base;

namespace OR_M_Data_Entities.Expressions
{
    public class ExpressionSelectQuery : ExpressionQuery
    {
        public ExpressionSelectQuery(ObjectMap map, DataFetching context)
            : base(context)
        {
            Map = map;
        }

        public ExpressionWhereJoinQuery SelectAll<T>() where T : class
        {
            // rename for asethetics
            if (Map != null && Map.BaseType != null && Map.BaseType == typeof(T))
            {
                throw new Exception("Cannot return more than one data type");
            }

            Table<T>();

            return new ExpressionWhereJoinQuery(Map, Context);
        }

        public ExpressionWhereJoinQuery Select<T>(Expression<Func<T, object>> selector) where T : class
        {
            // rename for asethetics
            if (Map != null && Map.BaseType != null && Map.BaseType == typeof(T))
            {
                throw new Exception("Cannot return more than one data type, please use Include<T> to return data");
            }

            Table<T>();

            // unselect all because we are using the expression to select what we want to return
            foreach (var table in Map.Tables)
            {
                table.UnSelectAll();
            }
            
            Map.DataReturnType = ObjectMapReturnType.Dynamic;

            LambdaResolver.ResolveSelectExpression(selector, Map);

            return new ExpressionWhereJoinQuery(Map, Context);
        }

        public ExpressionSelectQuery Include<T>(Expression<Func<T, object>> selector) where T : class
        {
            // rename for asethetics
            if (Map != null && Map.BaseType != null && Map.BaseType == typeof(T) && Map.DataReturnType != ObjectMapReturnType.Dynamic)
            {
                throw new Exception("Cannot return more than one data type, please use Include<T> to return data");
            }

            LambdaResolver.ResolveSelectExpression(selector, Map);

            return new ExpressionSelectQuery(Map, Context);
        }

        public ExpressionSelectQuery Include<T>() where T : class
        {
            Map.AddSingleTable(typeof(T), true);

            return new ExpressionSelectQuery(Map, Context);
        }

        public ExpressionWhereJoinQuery Distinct()
        {
            Map.IsDistinct = true;

            return new ExpressionWhereJoinQuery(Map, Context);
        }

        public ExpressionSelectQuery Take(int rows)
        {
            Map.Rows = rows;

            return new ExpressionSelectQuery(Map, Context);
        }

        public ExpressionJoinQuery InnerJoin<TParent, TChild>(Expression<Func<TParent, TChild, bool>> expression)
            where TParent : class
            where TChild : class
        {
            LambdaResolver.ResolveJoinExpression(expression, JoinType.Inner, Map);

            return new ExpressionJoinQuery(Map, Context);
        }

        public ExpressionJoinQuery LeftJoin<TParent, TChild>(Expression<Func<TParent, TChild, bool>> expression)
            where TParent : class
            where TChild : class
        {
            LambdaResolver.ResolveJoinExpression(expression, JoinType.Left, Map);

            return new ExpressionJoinQuery(Map, Context);
        }

        public ExpressionWhereQuery Where<T>(Expression<Func<T, bool>> expression) where T : class
        {
            LambdaResolver.ResolveWhereExpression(expression, Map);

            return new ExpressionWhereQuery(Map, Context);
        }

        public ExpressionOrderedQuery OrderBy<T>(Expression<Func<T, object>> selector)
        {
            LambdaResolver.ResolveOrderExpression(selector, Map, ObjectColumnOrderType.Ascending);

            return new ExpressionOrderedQuery(Map, Context);
        }

        public ExpressionOrderedQuery OrderByDescending<T>(Expression<Func<T, object>> selector)
        {
            LambdaResolver.ResolveOrderExpression(selector, Map, ObjectColumnOrderType.Descending);

            return new ExpressionOrderedQuery(Map, Context);
        }
    }
}
