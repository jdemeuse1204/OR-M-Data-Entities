using System;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Mapping
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PseudoKeyAttribute : SearchableForeignKeyAttribute
    {
        public PseudoKeyAttribute(string parentColumnName, string childColumnName)
        {
            ParentColumnName = parentColumnName;
            ChildColumnName = childColumnName;
        }

        /// <summary>
        /// Parent property name, not the column name if used. 
        /// </summary>
        public string ParentColumnName { get; private set; }

        /// <summary>
        /// Child property name, not the column name if used. 
        /// </summary>
        public string ChildColumnName { get; private set; }
    }
}
