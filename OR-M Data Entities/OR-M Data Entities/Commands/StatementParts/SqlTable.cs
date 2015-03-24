using System;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.StatementParts
{
    public sealed class SqlTable
    {
        public SqlTable(Type tableType)
        {
            TableName = DatabaseSchemata.GetTableName(tableType);
            TableType = tableType;
        }

        public string TableName { get; private set; }

        public Type TableType { get; private set; }
    }
}
