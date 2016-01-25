/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Configuration
{
    public sealed class KeyConfiguration
    {
        public int[] Int { get; set; }

        public Guid[] UniqueIdentifier { get; set; }

        public short[] SmallInt { get; set; }

        public long[] BigInt { get; set; }

        public string[] String { get; set; }

        public KeyConfiguration()
        {
            Int = new[] {0};
            UniqueIdentifier = new[] {Guid.Empty};
            SmallInt = new short[] {0};
            BigInt = new long[] {0};
            String = new[] {string.Empty};
        }
    }
}
