using System;

namespace OR_M_Data_Entities.Expressions.Query
{
    public class TableType : PartialTableType
    {
        public string Alias { get; private set; }

        public TableType(Type type, string alias, string propertyName)
            : base(type, propertyName)
        {
            Alias = alias;
        }
    }
}
