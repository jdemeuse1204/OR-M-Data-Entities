using System;
using System.Reflection;
using OR_M_Data_Entities.Expressions.Query.Tables;

namespace OR_M_Data_Entities.Expressions.Query.Columns
{
    public class SimpleColumn : PartialColumn
    {
        public SimpleColumn(Guid expressionQueryId, Type tableType, PropertyInfo property, string tableAlias, bool isPrimaryKey)
            : base(expressionQueryId, tableType, property)
        {
            Table = new AliasTable(expressionQueryId, tableType, tableAlias);
            IsPrimaryKey = isPrimaryKey;
        }

        public bool IsPrimaryKey { get; set; }

        public string GetTableAlias()
        {
            return Table != null ? ((AliasTable) Table).Alias : string.Empty;
        }
    }
}
