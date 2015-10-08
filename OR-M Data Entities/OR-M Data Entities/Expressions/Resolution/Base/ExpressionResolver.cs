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
using System.Reflection;
using OR_M_Data_Entities.Expressions.Resolution.SubQuery;
using OR_M_Data_Entities.Extensions;

namespace OR_M_Data_Entities.Expressions.Resolution.Base
{
    public abstract class ExpressionResolver
    {
        protected static string GetColumnName(MemberExpression expression)
        {
            // we want the actual property name, not the sql property name.  
            // The resolvers will grab the correct property name

            return expression.Member.Name;
        }

        protected static MemberInfo GetColumnInfo(MemberExpression expression)
        {
            if (expression.Expression is ParameterExpression)
            {
                return expression.Member;
            }

            return expression.Member;
        }

        protected static string GetTableName(MemberExpression expression)
        {
            if (expression.Expression is ParameterExpression)
            {
                return expression.Expression.Type.GetTableName();
            }

            return ((MemberExpression)expression.Expression).Member.Name;
        }

        protected static string GetTableAlias(MemberExpression expression)
        {
            if (expression.Expression is ParameterExpression)
            {
                return expression.Expression.Type.GetTableName();
            }

            return ((MemberExpression)expression.Expression).Member.Name;
        }

        #region Load Value
        protected static object GetValue(ConstantExpression expression, IExpressionQueryResolvable baseQuery)
        {
            return expression.Value ?? "IS NULL";
        }

        protected static object GetValue(MemberExpression expression, IExpressionQueryResolvable baseQuery)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            var value = getter();

            return value ?? "IS NULL";
        }

        protected static object GetValue(MethodCallExpression expression, IExpressionQueryResolvable baseQuery)
        {
            if (IsSubQuery(expression))
            {
                return SubQueryResolver.Resolve(expression, baseQuery);
            }

            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            var value = getter();

            return value ?? "IS NULL";
        }

        protected static object GetValue(UnaryExpression expression, IExpressionQueryResolvable baseQuery)
        {
            return GetValue(expression.Operand as dynamic, baseQuery);
        }
        #endregion

        protected static bool IsSubQuery(MethodCallExpression expression)
        {
            return
                expression.Arguments.OfType<MethodCallExpression>()
                    .Select(
                        methodCallExpression =>
                            methodCallExpression.IsExpressionQuery() || IsSubQuery(methodCallExpression))
                    .FirstOrDefault();
        }

        protected static bool IsExpressionQuery(object o)
        {
            return o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition()
                       .IsAssignableFrom(typeof(ExpressionQuery<>));
        }
    }
}
