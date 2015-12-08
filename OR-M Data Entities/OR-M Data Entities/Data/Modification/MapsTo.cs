using System;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data.Modification
{
    public class MapsTo
    {
        public MapsTo(Type parentType, string parentPropertyName, ForeignKeyAttribute foreignKey)
        {
            ParentType = parentType;
            ParentPropertyName = parentPropertyName;
            ForeignKey = foreignKey;
        }

        public readonly Type ParentType;

        public readonly string ParentPropertyName;

        public readonly ForeignKeyAttribute ForeignKey;

        public string AsVariable()
        {
            return string.Format("{0}{1}", ParentPropertyName, ForeignKey.ForeignKeyColumnName);
        }
    }
}
