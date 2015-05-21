using System;

namespace OR_M_Data_Entities.Expressions.Query
{
    public class TableInfo
    {
        public TableInfo(string tableName, string foreignKeyTableName, Type type, string queryTableName)
        {
            TableName = tableName;
            ForeignKeyTableName = foreignKeyTableName;
            Type = type;
            QueryTableName = queryTableName;
        }

        public string TableName { get; private set; }

        public string ForeignKeyTableName { get; private set; }

        public Type Type { get; private set; }

        public string QueryTableName { get; private set; }
    }
}
