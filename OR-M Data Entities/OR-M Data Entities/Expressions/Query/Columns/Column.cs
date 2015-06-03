using System;
using System.Reflection;

namespace OR_M_Data_Entities.Expressions.Query.Columns
{
    public class Column : PartialColumn
    {
        public Column(Guid expressionQueryId, Type tableType, PropertyInfo property, string alias, bool isPrimaryKey)
            : base(expressionQueryId, tableType, property)
        {
            Alias = alias;
            IsPrimaryKey = isPrimaryKey;
        }

        public string Alias { get; set; }

        public bool IsPrimaryKey { get; set; }
    }
}
