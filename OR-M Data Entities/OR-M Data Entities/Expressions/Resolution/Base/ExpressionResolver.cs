using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.SubQuery;

namespace OR_M_Data_Entities.Expressions.Resolution.Base
{
    public abstract class ExpressionResolver : DbQueryBase
    {
        protected ExpressionResolver(DbQueryBase query)
            : base(query)
        {
            
        }

        protected static string GetColumnName(MemberExpression expression)
        {
            if (expression.Expression is ParameterExpression)
            {
                return DatabaseSchemata.GetColumnName(expression.Member);
            }

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
                return DatabaseSchemata.GetTableName(expression.Expression.Type);
            }

            return ((MemberExpression)expression.Expression).Member.Name;
        }

        protected static string GetTableAlias(MemberExpression expression)
        {
            if (expression.Expression is ParameterExpression)
            {
                return DatabaseSchemata.GetTableName(expression.Expression.Type);
            }

            return ((MemberExpression)expression.Expression).Member.Name;
        }

        #region Load Value
        protected object GetValue(ConstantExpression expression)
        {
            return expression.Value ?? "IS NULL";
        }

        protected object GetValue(MemberExpression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            var value = getter();

            return value ?? "IS NULL";
        }

        protected object GetValue(MethodCallExpression expression)
        {
            if (IsSubQuery(expression))
            {
                return SubQueryResolver.Resolve(expression);
            }

            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            var value = getter();

            return value ?? "IS NULL";
        }

        protected object GetValue(UnaryExpression expression)
        {
            return GetValue(expression.Operand as dynamic);
        }
        #endregion

        protected bool IsSubQuery(MethodCallExpression expression)
        {
            return
                expression.Arguments.Select(argument => argument as MethodCallExpression)
                    .Select(
                        methodCallExpression =>
                            methodCallExpression.IsExpressionQuery() || IsSubQuery(methodCallExpression))
                    .FirstOrDefault();
        }

        protected bool IsExpressionQuery(object o)
        {
            return o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition()
                       .IsAssignableFrom(typeof(ExpressionQuery<>));
        }
    }
}
