using System;

namespace OR_M_Data_Entities.Mapping
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ForeignKey : Attribute
    {
        /// <summary>
        /// Use for identifying the foreign key
        /// </summary>
        /// <param name="parentTableType">Type</param>
        /// <param name="parentPropertyName">string</param>
        public ForeignKey(Type parentTableType, string parentPropertyName)
        {
            ParentTableType = parentTableType;
            ParentPropertyName = parentPropertyName;
        }

        /// <summary>
        /// Type of the parent table
        /// </summary>
        public Type ParentTableType { get; private set; }

        /// <summary>
        /// Parent property name, not the column name if used
        /// </summary>
        public string ParentPropertyName { get; private set; }
    }
}
