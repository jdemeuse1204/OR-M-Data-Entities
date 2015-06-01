using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data.Definition;

namespace OR_M_Data_Entities.Expressions.Query
{
    public class PartialTableType
    {
        public Type Type { get; private set; }

        // only applicable tp foreign Keys
        public string PropertyName { get; private set; }

        public string ActualTableName { get; private set; }

        public PartialTableType(Type type, string propertyName)
        {
            ActualTableName = DatabaseSchemata.GetTableName(type);
            Type = type;
            PropertyName = propertyName;
        }

        public static PartialTableType GetFromSelector<T, TKey>(Expression<Func<T, TKey>> outerKeySelector)
        {
            return new PartialTableType(null,null);
        }
    }
}
