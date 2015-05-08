using System.Linq.Expressions;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Query;

namespace OR_M_Data_Entities.Expressions.Resolution.Base
{
    public abstract class ExpressionResolver : DbQuery
    {
        protected ExpressionResolver(DbQuery query)
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

        protected static string GetTableName(MemberExpression expression)
        {
            if (expression.Expression is ParameterExpression)
            {
                return DatabaseSchemata.GetTableName(expression.Expression.Type);
            }

            return ((MemberExpression)expression.Expression).Member.Name;
        }
    }
}
