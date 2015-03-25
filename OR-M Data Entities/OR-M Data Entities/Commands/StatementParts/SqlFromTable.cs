/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using OR_M_Data_Entities.Commands.Secure;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.StatementParts
{
    public abstract class SqlFromTable : SqlSecureExecutable
    {
        protected string TableName { get; set; }

        protected SqlFromTable()
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
