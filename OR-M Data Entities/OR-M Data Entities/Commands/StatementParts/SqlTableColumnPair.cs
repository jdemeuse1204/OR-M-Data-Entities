/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using System;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.StatementParts
{
    public sealed class SqlTableColumnPair : SqlColumn, IEquatable<SqlTableColumnPair>
    {
        public Type Table { get; set; }

        public string GetTableName()
        {
            return DatabaseSchemata.GetTableName(Table);
        }

        public string GetSelectColumnText()
        {
            return GetColumnText(GetTableName());
        }

        public string GetSelectColumnTextWithAlias()
        {
            return GetColumnTextWithAlias(GetTableName());
        }

        public bool Equals(SqlTableColumnPair other)
        {
            return Table == other.Table && Column == other.Column;
        }
    }
}
