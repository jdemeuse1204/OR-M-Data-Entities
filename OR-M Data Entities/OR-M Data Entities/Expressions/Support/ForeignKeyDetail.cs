/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using System;
using System.Collections.Generic;

namespace OR_M_Data_Entities.Expressions.Support
{
    public sealed class ForeignKeyDetail
    {
        public Type ParentType { get; set; }

        public Type Type { get; set; }

        public string PropertyName { get; set; }

        public bool IsList { get; set; }

        public Type ListType { get; set; }

        public string[] PrimaryKeyDatabaseNames { get; set; }

        public Dictionary<int, List<int>> KeysSelectedHashCodeList { get; set; }

        public List<ForeignKeyDetail> ChildTypes { get; set; }
    }
}
