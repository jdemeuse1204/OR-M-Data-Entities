using System;
using System.Data;

namespace OR_M_Data_Entities.Expressions.Resolver
{
    public sealed class ExpressionSelectResult
    {
        public string ColumnName { get; set; }

        public Type ColumnType { get; set; }

        public string TableName { get; set; }

        public SqlDbType Transform { get; set; }

        public bool ShouldCast { get; set; }
    }
}
