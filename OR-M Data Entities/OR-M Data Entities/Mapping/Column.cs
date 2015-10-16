/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Mapping
{
    /// <summary>
    /// This attribute should be used when renaming a column on your table
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ColumnAttribute : SearchablePrimaryKeyAttribute
    {
        // SearchableKeyType needed for quick lookup in iterator
        public ColumnAttribute(string name)
            : base(SearchablePrimaryKeyType.Column)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public override bool IsPrimaryKey
        {
            get { return Name.ToUpper() == "ID"; }
        }
    }
}
