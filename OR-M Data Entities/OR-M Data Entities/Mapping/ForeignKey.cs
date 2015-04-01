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
        public ForeignKeyAttribute(string foreignKeyColumnName, string primaryKeyColumnName = "ID", bool isNullable = false)
        {
            ForeignKeyColumnName = foreignKeyColumnName;
            PrimaryKeyColumnName = primaryKeyColumnName;
            IsNullable = isNullable;
        }

        public ForeignKeyAttribute(string foreignKeyColumnName, bool isNullable)
        {
            ForeignKeyColumnName = foreignKeyColumnName;
            PrimaryKeyColumnName = "ID";
            IsNullable = isNullable;
        }

        public ForeignKeyAttribute(string foreignKeyColumnName, string primaryKeyColumnName)
        {
            ForeignKeyColumnName = foreignKeyColumnName;
            PrimaryKeyColumnName = primaryKeyColumnName;
        }

        /// <summary>
        /// Parent property name, not the column name if used
        /// </summary>
        public string ForeignKeyColumnName { get; private set; }

        public string PrimaryKeyColumnName { get; private set; }

        public bool IsNullable { get; private set; }
    }
}
