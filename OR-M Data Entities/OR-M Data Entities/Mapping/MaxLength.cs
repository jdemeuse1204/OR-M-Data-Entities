/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Mapping
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class MaxLengthAttribute : Attribute
    {
        public MaxLengthAttribute(int length, MaxLengthViolationType violationType = MaxLengthViolationType.Truncate)
        {
            Length = length;
            ViolationType = violationType;
        }

        public int Length { get; private set; }

        public MaxLengthViolationType ViolationType { get; private set; }
    }
}
