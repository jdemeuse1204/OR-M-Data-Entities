using System;
using System.Reflection;
using OR_M_Data_Entities.Expressions.Query.Tables;
using OR_M_Data_Entities.Expressions.Resolution.Base;

namespace OR_M_Data_Entities.Expressions.Query.Columns
{
    public class PartialColumn : IQueryPart
    {
        public PartialColumn(Guid expressionQueryId, Type tableType, PropertyInfo property)
        {
            ExpressionQueryId = expressionQueryId;
            Table = new DbTable(expressionQueryId, tableType);
            Property = property;
        }

        public PartialColumn(Guid expressionQueryId, Type tableType, string propertyName)
        {
            ExpressionQueryId = expressionQueryId;
            Table = new DbTable(expressionQueryId, tableType);
            _propertyName = propertyName;

            Property = tableType.GetProperty(propertyName);
        }

        public Guid ExpressionQueryId { get; set; }

        public DbTable Table { get; set; }

        public MemberInfo Property { get; set; }

        public bool IsPropertyList
        {
            get { return Property != null && Property.IsList(); }
        }

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
    }
}
