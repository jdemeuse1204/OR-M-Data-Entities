/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Mapping
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ForeignKeyAttribute : NonSelectableAttribute
    {
        /// <summary>
        /// Loads a class with the Foreign Key Property Set
        /// </summary>
        /// <param name="columnName"></param>
        public ForeignKeyAttribute(string foreignKeyColumnName, string primaryKeyColumnName = "ID")
        {
            ForeignKeyColumnName = foreignKeyColumnName;
            PrimaryKeyColumnName = primaryKeyColumnName;
        }

        /// <summary>
        /// Parent property name, not the column name if used
        /// </summary>
        public string ForeignKeyColumnName { get; private set; }

        public string PrimaryKeyColumnName { get; private set; }
    }
}
