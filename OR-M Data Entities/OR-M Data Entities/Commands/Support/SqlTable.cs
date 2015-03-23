using System;
using OR_M_Data_Entities.Commands.Secure;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.Support
{
    public abstract class SqlTable : SqlSecureExecutable
    {
        protected string TableName { get; set; }

        protected SqlTable()
        {
            TableName = string.Empty;
        }

        public void Table(string tableName)
        {
            TableName = tableName;
        }

        public void Table(Type tableType)
        {
            Table(DatabaseSchemata.GetTableName(tableType));
        }

        public void Table<T>()
        {
            Table(DatabaseSchemata.GetTableName(typeof(T)));
        }
    }
}
