/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Mapping
{
    /// <summary>
    /// One-Many - Reference Column in Other Table which links to PK of This Table
    /// One-One - Reference Column in This Table which links to PK of Other Table
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ForeignKeyAttribute : AutoLoadKeyAttribute
    {
        /// <summary>
        /// One-Many - Reference Column in Other Table which links to PK of This Table
        /// One-One - Reference Column in This Table which links to PK of Other Table
        /// </summary>
        /// <param name="foreignKeyColumnName">ONE-MANY: Reference Column in Other Table which links to PK of This Table, ONE-ONE: Reference Column in This Table which links to PK of Other Table</param>
        public ForeignKeyAttribute(string foreignKeyColumnName)
        {
            ForeignKeyColumnName = foreignKeyColumnName;
        }

        /// <summary>
        /// Parent property name, not the column name if used
        /// </summary>
        public string ForeignKeyColumnName { get; private set; }
    }
}
