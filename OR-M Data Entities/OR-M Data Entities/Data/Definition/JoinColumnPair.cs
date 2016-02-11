/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Reflection;

namespace OR_M_Data_Entities.Data.Definition
{
    public class JoinColumnPair
    {
        public JoinType JoinType { get; set; }

        public PropertyInfo ChildColumn { get; set; }

        public PropertyInfo ParentColumn { get; set; }

        public string JoinPropertyName { get; set; }

        public Type FromType { get; set; }
    }
}
