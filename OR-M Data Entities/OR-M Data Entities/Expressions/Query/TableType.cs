using System;

namespace OR_M_Data_Entities.Expressions.Query
{
    public class TableType : PartialTableType
    {
        public string Alias { get; private set; }

        public TableType(Type type, Guid expressionQueryId, string alias, string propertyName)
            : base(type, expressionQueryId, propertyName)
        {
            Alias = alias;
        }
    }
}
