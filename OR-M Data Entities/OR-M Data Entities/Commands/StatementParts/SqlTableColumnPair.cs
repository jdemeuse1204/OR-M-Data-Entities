using System;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.StatementParts
{
    public sealed class SqlTableColumnPair : SqlColumn
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
    }
}
