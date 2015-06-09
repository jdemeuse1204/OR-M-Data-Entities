using System;
using System.Reflection;
using OR_M_Data_Entities.Expressions.Query.Tables;
using OR_M_Data_Entities.Mapping;

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

        private string _newPropertyName;
        public string NewPropertyName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_newPropertyName) || NewProperty == null) return _newPropertyName;
                var columnAttribute = NewProperty.GetCustomAttribute<ColumnAttribute>();

                _newPropertyName = columnAttribute == null ? NewProperty.Name : columnAttribute.Name;

                return _newPropertyName;
            }
        }

        public bool IsNewPropertyList
        {
            get { return NewProperty != null && NewProperty.IsList(); }
        }

        public DbTable NewTable { get; set; }
    }
}
