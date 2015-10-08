/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Linq;
using System.Linq.Expressions;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query.Columns;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;

namespace OR_M_Data_Entities.Expressions.Resolution.Order
{
    public class OrderExpressionResolver : ExpressionResolver
    {
        public static void ResolveDescending<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector, SelectInfoResolutionContainer columns)
        {
            _resolve(keySelector, columns, OrderType.Descending);
        }

        public static void Resolve<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector, SelectInfoResolutionContainer columns)
        {
            _resolve(keySelector, columns, OrderType.Ascending);
        }

        private static void _resolve<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector,
            SelectInfoResolutionContainer columns, OrderType orderType)
        {
            var order = columns.Infos.Count(w => w.Order != null);
            var column = _find(keySelector.Body as MemberExpression, columns);

            column.Order = order;
            column.OrderType = orderType;
        }

        private static DbColumn _find(MemberExpression expression, SelectInfoResolutionContainer columns)
        {
            // what if we are selecting a foreign key, then we need to select all from that table
            return columns.Infos.First(
                    w =>
                        w.NewTable.Type == expression.Expression.Type &&
                        w.NewProperty.Name == expression.Member.Name);
        }
    }
}
