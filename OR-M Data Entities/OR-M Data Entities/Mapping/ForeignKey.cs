/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Mapping
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ForeignKeyAttribute : Attribute
    {
        /// <summary>
        /// Use for identifying the foreign key
        /// </summary>
        /// <param name="parentTableType">Type</param>
        /// <param name="parentPropertyName">string</param>
        public ForeignKeyAttribute(Type parentTableType, string parentPropertyName)
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
