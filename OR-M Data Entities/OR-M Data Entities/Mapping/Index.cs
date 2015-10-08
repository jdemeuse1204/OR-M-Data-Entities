/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Mapping
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class Index : Attribute
    {
        // SearchableKeyType needed for quick lookup in iterator
        public Index(int value)
        {
            Value = value;
        }

        public int Value { get; private set; }
    }
}
