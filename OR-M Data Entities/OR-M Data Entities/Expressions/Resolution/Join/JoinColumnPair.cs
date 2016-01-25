/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query.Columns;

namespace OR_M_Data_Entities.Expressions.Resolution.Join
{
    public class JoinColumnPair
    {
        public JoinType JoinType { get; set; }

        public PartialColumn ChildColumn { get; set; }

        public PartialColumn ParentColumn { get; set; }

        public string JoinPropertyName { get; set; }

        public Type FromType { get; set; }
    }
}
