using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.StatementParts;
using OR_M_Data_Entities.Commands.Transform;

namespace OR_M_Data_Entities.Data
{
    public static class DatabaseOperations
    {
        #region Casting and Converting
        public static bool IsCasting(object expression)
        {
            return _isTransform(expression, "Cast");
        }

        public static bool IsConverting(object expression)
        {
            return _isTransform(expression, "Convert");
        }

        private static bool _isTransform(object expression, string methodName)
        {
            if (!(expression is MethodCallExpression)) return false;

            var methodCallExpression = (MethodCallExpression)expression;

            return methodCallExpression.Method.DeclaringType == typeof(DbFunctions) &&
                String.Equals(methodCallExpression.Method.Name, methodName, StringComparison.CurrentCultureIgnoreCase);
        }

        public static string GetComparisonString(SqlWhere where)
        {
            if (where.ComparisonType == ComparisonType.Contains)
            {
                return where.ObjectCompareValue.IsList() ? " {0} IN ({1}) " : " {0} LIKE {1}";
            }

            switch (where.ComparisonType)
            {
                case ComparisonType.BeginsWith:
                case ComparisonType.EndsWith:
                    return " {0} LIKE {1}";
                case ComparisonType.Equals:
                    return "{0} = {1}";
                case ComparisonType.EqualsIgnoreCase:
                    return "";
                case ComparisonType.EqualsTruncateTime:
                    return "";
                case ComparisonType.GreaterThan:
                    return "{0} > {1}";
                case ComparisonType.GreaterThanEquals:
                    return "{0} >= {1}";
                case ComparisonType.LessThan:
                    return "{0} < {1}";
                case ComparisonType.LessThanEquals:
                    return "{0} <= {1}";
                case ComparisonType.NotEqual:
                    return "{0} != {1}";
                default:
                    throw new ArgumentOutOfRangeException("comparison");
            }
        }
        #endregion
    }
}
