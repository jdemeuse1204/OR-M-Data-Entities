using System;

namespace OR_M_Data_Entities.Expressions.Query
{
    public class TableType
    {
        public Type Type { get; private set; }

        public string Alias { get; private set; }

        public TableType(Type type, string alias)
        {
            Type = type;
            Alias = alias;
        }
    }
}
