using System.Data;
using System.Reflection;
using OR_M_Data_Entities.Data.PayloadOperations.ObjectFunctions;

namespace OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping
{
    public class ObjectColumn : ObjectFunctionTextBuilder
    {
        public string Name { get; private set; }

        public SqlDbType DataType { get; private set; }

        public ObjectColumn(PropertyInfo memberInfo)
        {
            Name = DatabaseSchemata.GetColumnName(memberInfo);

            DataType = DatabaseSchemata.GetSqlDbType(memberInfo.PropertyType);
        }
    }
}
