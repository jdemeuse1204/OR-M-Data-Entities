/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Collections;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;

namespace OR_M_Data_Entities.Expressions.Resolution.Join
{
    public class JoinExpressionResolver : ExpressionResolver
    {
        public static void Resolve<TOuter, TInner, TKey, TResult>(
            ExpressionQuery<TOuter> outer,
            ExpressionQuery<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector,
            JoinType joinType,
            JoinResolutionContainer joinResolution,
            TableCollection tables, 
            Guid expressionQueryId)
        {
            // If we are doing a join here do not worry about foreign keys.  
            var computedChildTableName = tables.Find(typeof(TInner), expressionQueryId).Alias;
            var computedParentTableName = tables.Find(typeof(TOuter), expressionQueryId).Alias;

            joinResolution.Add(new JoinTablePair(
                expressionQueryId,
                typeof (TOuter),
                typeof (TInner),
                !_hasInnerJoin(joinType),
                computedParentTableName,
                computedChildTableName,
                GetColumnName(outerKeySelector.Body as dynamic),
                GetColumnName(innerKeySelector.Body as dynamic),
                null));
        }

        private static Type _resolveType<T, TKey>(Expression<Func<T, TKey>> expression)
        {
            if (!expression.Parameters[0].Type.IsAnonymousType()) return expression.Parameters[0].Type;

            var newExpression = null as NewExpression;
            var field = expression.Body as MemberExpression;

            if (field == null) throw new Exception("Could not resolve field in JoinExpressionResolver");

            if (newExpression != null)
            {
                for (var i = 0; i < newExpression.Arguments.Count; i++)
                {
                    var argument = newExpression.Arguments[i];
                    var member = newExpression.Members[i];

                    if (member.Name == field.Member.Name)
                    {
                        return ((MemberExpression)argument).Expression.Type;
                    }
                }
            }

            throw new Exception("Could not resolve type");
        }

        private static bool _hasInnerJoin(JoinType joinType)
        {
            switch (joinType)
            {
                case JoinType.ForeignKeyInner:
                case JoinType.Inner:
                case JoinType.PseudoKeyInner:
                    return true;
                default:
                    return false;
            }
        }
    }
}
