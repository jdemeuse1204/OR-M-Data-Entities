using System;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Types.Base;

namespace OR_M_Data_Entities.Expressions.Types
{
    public class ForeignKeySelectExpression : SqlExpressionResolvableBase
    {
        public ForeignKeySelectExpression(ExpressionQuery query)
            : base(query)
        {

        }

        public override void Resolve()
        {
            // Turn the Select Lambda Statements into Sql
            var selects = ResolveSelectsList();

            // Resolve Joins And Selects for FKs
            // make sure all joins and fields are incorporated into sql
            var foreignKeyJoins = DatabaseSchemata.GetForeignKeyJoinsRecursive(Query.ReturnDataType);

            foreach (var distinctSelectType in foreignKeyJoins.SelectedTypes)
            {
                selects.AddRange(DatabaseSchemata.GetTableColumnPairsFromTable(distinctSelectType).Where(w => !selects.Contains(w)).ToList());
            }

            var selectTopModifier = Query.TakeTopRows == -1 ? string.Empty : string.Format(" TOP {0} ", Query.TakeTopRows);
            var selectDistinctModifier = Query.IsDistinct ? "DISTINCT" : string.Empty;

            // add the select modifier
            Query.Sql += string.Format(" SELECT {0}{1} ", selectDistinctModifier, selectTopModifier);

            var selectText = selects.Aggregate(string.Empty, (current, item) => current + string.Format("{0},", item.GetSelectColumnTextWithAlias()));

            // trim the ending comma
            Query.Sql += selectText.TrimEnd(',');

            Query.Sql += string.Format(" FROM [{0}] ", Query.FromTableName);

            var innerJoinText = foreignKeyJoins.GetSql();

            if (!string.IsNullOrWhiteSpace(innerJoinText))
            {
                Query.Sql += innerJoinText;
            }
        }
    }
}
