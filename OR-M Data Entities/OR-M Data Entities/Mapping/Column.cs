/*
 * OR-M Data Entities v2.2
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Mapping
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
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
