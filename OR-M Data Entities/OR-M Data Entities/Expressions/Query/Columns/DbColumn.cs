using System;
using System.Reflection;
using OR_M_Data_Entities.Expressions.Query.Tables;

namespace OR_M_Data_Entities.Expressions.Query.Columns
{
    public class DbColumn : SimpleColumn
    {
        public DbColumn(Guid expressionQueryId, Type tableType, PropertyInfo property, string tableAlias, bool isPrimaryKey, int ordinal)
            : base(expressionQueryId, tableType, property, tableAlias, isPrimaryKey)
        {
            Ordinal = ordinal;
            NewProperty = property;
            NewTable = new DbTable(expressionQueryId, tableType);
            IsSelected = true;
        }

        public int Ordinal { get; set; }

        public bool IsSelected { get; set; }

        public MemberInfo NewProperty { get; set; }

        public bool IsNewPropertyList
        {
            get { return NewProperty != null && NewProperty.IsList(); }
        }

        public DbTable NewTable { get; set; }
    }
}
