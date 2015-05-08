using System;
using System.Linq;
using System.Linq.Expressions;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class SubQueryResolver
    {
        public static object Resolve(MethodCallExpression expression)
        {
            object result = null;
            Type type = null;

            // TODO make so subquery can contain a subquery..... might work already!?

            _resolveMethodCall(expression, ref result, ref type);

            return result;
        }

        private static void _resolveMethodCall(MethodCallExpression expression, ref object query, ref Type type)
        {
            foreach (var argument in expression.Arguments)
            {
                var nextMethodCallExpression = argument as MethodCallExpression;

                if (nextMethodCallExpression != null)
                {
                    // remove recursion
                    _resolveMethodCall(nextMethodCallExpression,ref query, ref type);
                }
            }

            //while (true)
            //{


            //    expression = expression.Arguments.FirstOrDefault(w => w.Type == typeof(MethodCallExpression)) as MethodCallExpression;

            //    if (expression == null) break;
            //}

            switch (expression.Method.Name)
            {
                case "Select":
                    _resolveSelect(expression, query);
                    break;
                case "Where":
                    var unaryExpression = expression.Arguments.Last() as UnaryExpression;

                    typeof(ExpressionQueryExtensions).GetMethod("Where").MakeGenericMethod(type).Invoke(null, new object[] { query, unaryExpression.Operand });
                    break;
                case "From":

                    type = expression.Type.GenericTypeArguments[0];
                    var expressionQuery = typeof(ExpressionQuery<>);
                    var creationType = expressionQuery.MakeGenericType(type);

                    query = Activator.CreateInstance(creationType);
                    break;
                case "First":
                    break;
                case "FirstOrDefault":
                    break;
                case "InnerJoin":
                    break;
                case "LeftJoin":
                    break;
            }  
        }

        private static void _resolveSelect(MethodCallExpression expression, object query)
        {
            
        }
    }
}
