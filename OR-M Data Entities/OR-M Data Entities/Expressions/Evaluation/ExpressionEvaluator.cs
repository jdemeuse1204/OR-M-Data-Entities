using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.Transform;

namespace OR_M_Data_Entities.Expressions.Evaluation
{
    public sealed class ExpressionEvaluator
    {
        public static bool IsConverting(object expression)
        {
            if (!(expression is MethodCallExpression)) return false;

            return ((MethodCallExpression)expression).Method.DeclaringType == typeof(Conversion);
        }

        public static bool IsCasting(object expression)
        {
            if (!(expression is MethodCallExpression)) return false;

            return ((MethodCallExpression)expression).Method.DeclaringType == typeof(Cast);
        }

        public static SqlDbType GetTransformType(MethodCallExpression expression)
        {
            return (from arg in expression.Arguments where arg.Type == typeof(SqlDbType) select ((ConstantExpression)arg).Value into value select value is SqlDbType ? (SqlDbType)value : SqlDbType.VarChar).FirstOrDefault();
        }

        public static ComparisonType GetComparisonType(string methodName)
        {
            switch (methodName.Replace(" ", "").ToUpper())
            {
                case "EQUALS":
                    return ComparisonType.Equals;
                case "NOTEQUALS":
                    return ComparisonType.NotEqual;
                case "LESSTHAN":
                    return ComparisonType.LessThan;
                case "GREATERTHAN":
                    return ComparisonType.GreaterThan;
                case "LESSTHANEQUALS":
                    return ComparisonType.LessThanEquals;
                case "GREATERTHANEQUALS":
                    return ComparisonType.GreaterThanEquals;
                case "CONTAINS":
                    return ComparisonType.Contains;
                case "STARTSWITH":
                    return ComparisonType.BeginsWith;
                case "ENDSWITH":
                    return ComparisonType.EndsWith;
                default:
                    throw new Exception("ExpressionType not in tree");
            }
        }

        public static ComparisonType GetComparisonType(ExpressionType expresssionType)
        {
            switch (expresssionType)
            {
                case ExpressionType.Equal:
                    return ComparisonType.Equals;
                case ExpressionType.GreaterThan:
                    return ComparisonType.GreaterThan;
                case ExpressionType.GreaterThanOrEqual:
                    return ComparisonType.GreaterThanEquals;
                case ExpressionType.LessThan:
                    return ComparisonType.LessThan;
                case ExpressionType.LessThanOrEqual:
                    return ComparisonType.LessThanEquals;
                case ExpressionType.NotEqual:
                case ExpressionType.Not:
                    return ComparisonType.NotEqual;
                default:
                    throw new Exception("ExpressionType not in tree");
            }
        }

        public static bool HasLeft(Expression expression)
        {
            return expression.NodeType == ExpressionType.And
                || expression.NodeType == ExpressionType.AndAlso
                || expression.NodeType == ExpressionType.Or
                || expression.NodeType == ExpressionType.OrElse;
        }
    }
}
