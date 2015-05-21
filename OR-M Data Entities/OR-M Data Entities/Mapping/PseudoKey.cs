using System;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Mapping
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PseudoKeyAttribute : AutoLoadKeyAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentTableColumnName">Column from THIS table</param>
        /// <param name="childTableColumnName">Column from your Join Table</param>
        public PseudoKeyAttribute(string parentTableColumnName, string childTableColumnName)
        {
            ParentTableColumnName = parentTableColumnName;
            ChildTableColumnName = childTableColumnName;
        }

        public string ParentTableColumnName { get; private set; }

        public string ChildTableColumnName { get; private set; }
    }
}