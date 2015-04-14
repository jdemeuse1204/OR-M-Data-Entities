﻿/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using System.Linq;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Types.Base;

namespace OR_M_Data_Entities.Expressions.Types
{
    public sealed class ForeignKeySelectJoinExpression : SqlExpressionResolvableBase
    {
        public ForeignKeySelectJoinExpression(ExpressionQuery query)
            : base(query)
        {
            
        }

        public override DataQueryType Resolve()
        {
            // Turn the Select Lambda Statements into Sql
            var selects = ResolveSelectsList();
            var joins = ResolveJoinsList();

            // Resolve Joins And Selects for FKs
            // make sure all joins and fields are incorporated into sql
            DatabaseSchemata.GetForeignKeyJoinsRecursive(Query.ReturnDataType, joins);

            foreach (var distinctSelectType in joins.SelectedTypes)
            {
                selects.AddRange(DatabaseSchemata.GetTableColumnPairsFromTable(distinctSelectType.Value, distinctSelectType.Key).Where(w => !selects.Contains(w)).ToList());
            }

            var selectTopModifier = Query.TakeTopRows == -1 ? string.Empty : string.Format(" TOP {0} ", Query.TakeTopRows);
            var selectDistinctModifier = Query.IsDistinct ? "DISTINCT" : string.Empty;

            // add the select modifier
            Query.Sql += string.Format(" SELECT {0}{1} ", selectDistinctModifier, selectTopModifier);

            var selectText = selects.Aggregate(string.Empty, (current, item) => current + string.Format("{0},", item.GetSelectColumnTextWithAlias()));

            // trim the ending comma
            Query.Sql += selectText.TrimEnd(',');

            Query.Sql += string.Format(" FROM [{0}] ", Query.FromTableName);

            // must be done in this order
            var joinText = joins.GetSql();

            if (!string.IsNullOrWhiteSpace(joinText))
            {
                Query.Sql += joinText;
            }

            return DataQueryType.ForeignKeys;
        }
    }
}
