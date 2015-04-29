using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using OR_M_Data_Entities.Expressions.Resolution.Base;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class SqlExpressionWhereResolver : SqlExpressionResolver
    {
        public static void Resolve<T>(Expression<Func<T, bool>> expression, SqlQuery query)
        {
            _evaluateExpressionTree(expression.Body, query);
        }

        private static void _evaluateExpressionTree(Expression expression, SqlQuery query)
        {
            if (HasLeft(expression))
            {
                _evaluateWhere(((dynamic)expression).Right, null, query);

                _evaluateExpressionTree(((BinaryExpression)expression).Left, query);
            }
            else
            {
                _evaluateWhere((dynamic)expression, null, query);
            }
        }

        private static void _evaluateWhere(object expression, ExpressionType? nodeType, SqlQuery query)
        {
            _evaluateWhere(expression as dynamic, nodeType, query);
        }

        private static void _evaluateWhere(MethodCallExpression expression, ExpressionType? nodeType, SqlQuery query)
        {
            var argsHaveParameter = false;
            var compareName = string.Format("{0}{1}",
                nodeType != null && nodeType.Value == ExpressionType.Not ? "NOT" : string.Empty,
                expression.Method.Name);

            foreach (var arg in expression.Arguments.Where(HasParameter))
            {
                argsHaveParameter = true;
                var compareValue = GetValue(expression.Object as dynamic);
                var tableAndColumnName = GetTableAndColumnName(arg);
                var compareString = _getComparisonString(compareName,
                    string.Format("[{0}].[{1}]", tableAndColumnName[0], tableAndColumnName[1]), compareValue, query);

                query.Where.Add(compareString);
                break;
            }

            if (!argsHaveParameter)
            {
                var compareValue = GetValue(expression.Arguments[0] as dynamic);
                var tableAndColumnName = GetTableAndColumnName(expression.Object);
                var compareString = _getComparisonString(compareName,
                    string.Format("[{0}].[{1}]", tableAndColumnName[0], tableAndColumnName[1]), compareValue, query);

                query.Where.Add(compareString);
            }
        }



        private static void _evaluateWhere(BinaryExpression expression, ExpressionType? nodeType, SqlQuery query)
        {
            var compareName = string.Format("{0}{1}",
                nodeType != null && nodeType.Value == ExpressionType.Not ? "NOT" : string.Empty,
                expression.Type == typeof(bool) ? "EQUALS" : expression.Method.Name);
            var compareValue = GetCompareValue(expression, SqlDbType.VarChar);
            var tableAndColumnName = GetTableAndColumnName(expression);
            var compareString = _getComparisonString(compareName,
                string.Format("[{0}].[{1}]", tableAndColumnName[0], tableAndColumnName[1]), compareValue, query);

            query.Where.Add(compareString);
        }

        private static void _evaluateWhere(UnaryExpression expression, ExpressionType? nodeType, SqlQuery query)
        {
            _evaluateWhere(expression.Operand as dynamic, expression.NodeType, query);

        }

        protected static string _addParameter(object value, SqlQuery query, string comparisonName)
        {
            var compareValue = _resolveContainsObject(value, comparisonName);

            var parameter = query.GetNextParameter();
            query.AddParameter(parameter, compareValue);

            return parameter;
        }

        private static string _resolveContainsObject(object compareValue, string comparisonName)
        {
            var compareValueAsString = Convert.ToString(compareValue);

            switch (comparisonName)
            {
                case "CONTAINS":
                    return "%" + compareValueAsString + "%";
                case "STARTSWITH":
                case "NOTSTARTSWITH":
                    return compareValueAsString + "%";
                case "ENDSWITH":
                case "NOTENDSWITH":
                    return "%" + compareValueAsString;
                default:
                    return compareValueAsString;
            }
        }

        private static string _getComparisonString(string methodName, string tableColumnName, object compareValue, SqlQuery query)
        {
            string comparisonString;
            var isCompareValueList = compareValue.IsList();
            var comparisonName = methodName.ToUpper().Contains("EQUALITY") ? "EQUALS" : methodName.ToUpper().Replace(" ", "");

            if (methodName.ToUpper() == "CONTAINS")
            {
                return string.Format(isCompareValueList ? " {0} IN ({1}) " : " {0} LIKE {1}", tableColumnName,
                    isCompareValueList
                        ? EnumerateList(compareValue as IEnumerable, query)
                        : _addParameter(compareValue, query, comparisonName));
            }

            if (methodName.ToUpper() == "NOTCONTAINS")
            {
                return string.Format(isCompareValueList ? " {0} NOT IN ({1}) " : " {0} NOT LIKE {1}", tableColumnName,
                    isCompareValueList
                        ? EnumerateList(compareValue as IEnumerable, query)
                        : _addParameter(compareValue, query, comparisonName));
            }

            switch (comparisonName)
            {
                case "STARTSWITH":
                case "ENDSWITH":
                    comparisonString = " {0} LIKE {1}";
                    break;
                case "NOTSTARTSWITH":
                case "NOTENDSWITH":
                    comparisonString = " {0} NOT LIKE {1}";
                    break;
                case "EQUALS":
                    comparisonString = " {0} = {1}";
                    break;
                case "GREATERTHAN":
                    comparisonString = " {0} > {1}";
                    break;
                case "GREATERTHANEQUALS":
                    comparisonString = " {0} >= {1}";
                    break;
                case "LESSTHAN":
                    comparisonString = " {0} < {1}";
                    break;
                case "LESSTHANEQUALS":
                    comparisonString = " {0} <= {1}";
                    break;
                case "NOTEQUALS":
                    comparisonString = " {0} != {1}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Cannot resolve comparison type {0}",
                        comparisonName));
            }

            return string.Format(comparisonString, tableColumnName, _addParameter(compareValue, query, comparisonName));
        }
    }
}
