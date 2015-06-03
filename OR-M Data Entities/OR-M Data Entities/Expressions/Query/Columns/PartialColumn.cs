using System;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Resolution.Where.Base;

namespace OR_M_Data_Entities.Expressions.Query.Columns
{
    public class PartialColumn : IQueryPart
    {
        public PartialColumn(Guid expressionQueryId, Type tableType, PropertyInfo property)
        {
            ExpressionQueryId = expressionQueryId;
            TableType = tableType;
            Property = property;
        }

        public Guid ExpressionQueryId { get; set; }

        public Type TableType { get; set; }

        public PropertyInfo Property { get; set; }

        public string _propertyName;

        public string PropertyName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_propertyName) && Property != null)
                {
                    _propertyName = Property.Name;
                }
                return _propertyName;
            }
        }

        private string _tableName;

        public string TableName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_tableName) && TableType != null)
                {
                    _tableName = DatabaseSchemata.GetTableName(TableType);
                }
                return _tableName;
            }
        }
    }
}
