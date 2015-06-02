using System;
using OR_M_Data_Entities.Data.Definition;

namespace OR_M_Data_Entities.Expressions.Query
{
    public class PartialTableType
    {
        public Guid ExpressionQueryId { get; private set; }

        public Type Type { get; private set; }

        // only applicable tp foreign Keys
        public string PropertyName { get; private set; }

        public string ActualTableName { get; private set; }

        public PartialTableType(Type type, Guid expressionQueryI, string propertyName)
        {
            ActualTableName = DatabaseSchemata.GetTableName(type);
            Type = type;
            PropertyName = propertyName;
            ExpressionQueryId = expressionQueryI;
        }
    }
}
