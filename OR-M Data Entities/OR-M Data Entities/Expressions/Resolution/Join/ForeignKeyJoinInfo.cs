using System.Reflection;

namespace OR_M_Data_Entities.Expressions.Resolution.Join
{
    public class ForeignKeyJoinInfo
    {
        public PropertyInfo Property { get; set; }

        public string ParentPropertyName { get; set; }

        public string ChildPropertyName { get; set; }

        public string ActualParentTableName { get; set; }

        public string ActualChildTableName { get; set; }
    }
}
