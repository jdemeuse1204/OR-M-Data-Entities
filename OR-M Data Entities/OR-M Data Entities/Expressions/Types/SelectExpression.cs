using System.Linq;
using OR_M_Data_Entities.Expressions.Support;
using OR_M_Data_Entities.Expressions.Types.Base;

namespace OR_M_Data_Entities.Expressions.Types
{
    public class SelectExpression : SqlExpressionResolvableBase
    {
        public SelectExpression(ExpressionQuery query)
            : base(query)
        {
            
        }

        public override SqlExpressionType Resolve()
        {
            // Turn the Select Lambda Statements into Sql
            var selects = ResolveSelectsList();

            var selectTopModifier = Query.TakeTopRows == -1 ? string.Empty : string.Format(" TOP {0} ", Query.TakeTopRows);
            var selectDistinctModifier = Query.IsDistinct ? "DISTINCT" : string.Empty;

            // add the select modifier
            Query.Sql += string.Format(" SELECT {0}{1} ", selectDistinctModifier, selectTopModifier);

            var selectText = selects.Aggregate(string.Empty, (current, item) => current + string.Format("{0},", item.GetSelectColumnTextWithAlias()));

            // trim the ending comma
            Query.Sql += selectText.TrimEnd(',');

            Query.Sql += string.Format(" FROM [{0}] ", Query.FromTableName);

            return SqlExpressionType.Select;
        }
    }
}
