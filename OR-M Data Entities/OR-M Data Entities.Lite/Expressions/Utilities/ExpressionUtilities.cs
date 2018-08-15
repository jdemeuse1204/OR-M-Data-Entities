using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Expressions.Utilities
{
    public static class ExpressionUtilities
    {
        public static bool IsTerminatingExpressionType(ExpressionType expressionType)
        {
            return expressionType == ExpressionType.Call ||
                expressionType == ExpressionType.Equal ||
                expressionType == ExpressionType.NotEqual ||
                expressionType == ExpressionType.LessThan ||
                expressionType == ExpressionType.LessThanOrEqual ||
                expressionType == ExpressionType.GreaterThan ||
                expressionType == ExpressionType.GreaterThanOrEqual;
        }

        public static bool IsBinaryNodeExpressionType(ExpressionType expressionType)
        {
            return expressionType == ExpressionType.AndAlso ||
                expressionType == ExpressionType.OrElse;
        }

        public static bool IsEqualityExpression(ExpressionType expressionType)
        {
            return expressionType == ExpressionType.AndAlso ||
                expressionType == ExpressionType.OrElse;
        }
    }
}
