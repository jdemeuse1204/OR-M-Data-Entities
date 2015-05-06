using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions;

namespace LambdaResolver
{
    public abstract class SqlExpressionResolver
    {
        protected static object GetCompareValue(BinaryExpression expression, SqlDbType transformType)
        {
            var leftSideHasParameter = HasParameter(expression.Left);

            return GetValue(leftSideHasParameter ? expression.Right as dynamic : expression.Left as dynamic);
        }

        protected static string[] GetTableAndColumnName(Expression expression, string columnName = "")
        {
            var binaryExpression = expression as BinaryExpression;

            if (binaryExpression != null)
            {
                return GetTableAndColumnName(binaryExpression.Left);
            }

            var memberExpression = expression as MemberExpression;

            if (memberExpression != null && memberExpression.Expression.NodeType != ExpressionType.Parameter)
            {
                return GetTableAndColumnName(memberExpression.Expression, memberExpression.Member.Name);
            }

            return string.IsNullOrWhiteSpace(columnName)
                ? new[]
                {
                    DatabaseSchemata.GetTableName(memberExpression.Expression.Type),
                    DatabaseSchemata.GetColumnName(((MemberExpression) expression).Member)
                }
                : new[]
                {
                    ((MemberExpression) expression).Member.Name, 
                    columnName
                };

        }

        protected static object GetValue(ConstantExpression expression)
        {
            return expression.Value;
        }

        protected static object GetValue(MemberExpression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }

        protected static object GetValue(MethodCallExpression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }

        protected static object GetValue(UnaryExpression expression)
        {
            return GetValue(expression.Operand as dynamic);
        }

        protected static string EnumerateList(IEnumerable list, SqlQuery query)
        {
            var result = "";

            foreach (var item in list)
            {
                var parameter = query.GetNextParameter();
                query.AddParameter(parameter, item);

                result += parameter + ",";
            }

            return result.TrimEnd(',');
        }

        protected static bool HasParameter(object expression)
        {
            return HasParameter(expression as dynamic);
        }

        protected static bool HasParameter(MethodCallExpression expression)
        {
            var e = expression.Object;

            return e != null ? HasParameter(expression.Object as dynamic) : expression.Arguments.Select(arg => HasParameter(arg as dynamic)).Any(hasParameter => hasParameter);
        }

        protected static bool HasParameter(ConstantExpression expression)
        {
            return false;
        }

        protected static bool HasParameter(UnaryExpression expression)
        {
            return expression == null ? false : HasParameter(expression.Operand as dynamic);
        }

        protected static bool HasParameter(ParameterExpression expression)
        {
            return true;
        }

        protected static bool HasParameter(MemberExpression expression)
        {
            return HasParameter(expression.Expression as dynamic);
        }

        protected static bool HasLeft(Expression expression)
        {
            return expression.NodeType == ExpressionType.And
                || expression.NodeType == ExpressionType.AndAlso
                || expression.NodeType == ExpressionType.Or
                || expression.NodeType == ExpressionType.OrElse;
        }
    }
}
