using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Expressions.Resolution.SubQuery
{
    public class SubQueryResolver
    {
        public static object Resolve(MethodCallExpression expression)
        {
            object result = null;
            Type type = null;

            // TODO make so subquery can contain a subquery..... might work already!?

            _resolve(expression, ref result, ref type);

            return result;
        }

        private static void _resolve(MethodCallExpression expression, ref object query, ref Type type)
        {
            foreach (var argument in expression.Arguments)
            {
                var nextMethodCallExpression = argument as MethodCallExpression;

                if (nextMethodCallExpression != null)
                {
                    // remove recursion
                    _resolve(nextMethodCallExpression, ref query, ref type);

                    switch (expression.Method.Name.ToUpper())
                    {
                        case "SELECT":
                            var selectExpression = expression.Arguments.Last() as UnaryExpression;
                            var selectType = ((LambdaExpression) selectExpression.Operand).ReturnType;

                            _resolveSelect(selectExpression, query, type, selectType);
                            break;
                        case "WHERE":
                            _resolveWhere(expression.Arguments.Last() as UnaryExpression, query, type);
                            break;
                    }
                }
            }

            if (expression.Method.Name.ToUpper() == "FROM")
            {
                _resolveFrom(expression, ref query, ref type);
            }
        }

        private static void _resolveWhere(UnaryExpression expression, object query, Type type)
        {
            typeof(ExpressionQueryExtensions).GetMethod("Where").MakeGenericMethod(type).Invoke(query, new[] { query, expression.Operand });
        }

        private static void _resolveSelect(UnaryExpression expression, object query, Type type, Type returnType)
        {
            typeof(ExpressionQueryExtensions).GetMethod("Select").MakeGenericMethod(type, returnType).Invoke(query, new[] { query, expression.Operand });
        }

        private static void _resolveFrom(Expression expression, ref object query, ref Type type)
        {
            type = expression.Type.GenericTypeArguments[0];
            var expressionQuery = typeof(ExpressionQueryResolvable<>);
            var creationType = expressionQuery.MakeGenericType(type);

            query = Activator.CreateInstance(creationType);
        }
    }
}
