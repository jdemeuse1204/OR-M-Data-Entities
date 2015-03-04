using System.Data;
using OR_M_Data_Entities.Expressions.Resolver;

namespace OR_M_Data_Entities.Commands.Transform
{
    public abstract class DataTransform : ExpressionResolver
    {
        protected string Cast(ExpressionWhereResult expressionWhereResult)
        {
            return string.Format(" CAST([{0}].[{1}] as {2}) ",
                expressionWhereResult.TableName,
                expressionWhereResult.PropertyName,
                expressionWhereResult.Transform);
        }

        protected string Cast(ExpressionSelectResult expressionSelectResult, bool includeAlias = true)
        {
            return string.Format(includeAlias ? " CAST([{0}].[{1}] as {2}) as '{1}'" : " CAST([{0}].[{1}] as {2})", expressionSelectResult.TableName, expressionSelectResult.ColumnName, expressionSelectResult.Transform);
        }

        protected string Cast(string parameter, SqlDbType targeTransformType)
        {
            return string.Format(" CAST({0} as {1}) ", parameter, targeTransformType);
        }
    }
}
