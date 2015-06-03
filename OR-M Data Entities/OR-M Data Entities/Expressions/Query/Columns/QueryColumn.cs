using System;
using System.Reflection;

namespace OR_M_Data_Entities.Expressions.Query.Columns
{
    public class QueryColumn : Column
    {
        public QueryColumn(Guid expressionQueryId, Type tableType, PropertyInfo property, string alias, bool isPrimaryKey, int ordinal)
            : base(expressionQueryId, tableType, property, alias, isPrimaryKey)
        {
            Ordinal = ordinal;
            NewProperty = property;
            NewTableType = tableType;
        }

        public int Ordinal { get; set; }

        public PropertyInfo NewProperty { get; set; }

        public Type NewTableType { get; set; }
    }
}
