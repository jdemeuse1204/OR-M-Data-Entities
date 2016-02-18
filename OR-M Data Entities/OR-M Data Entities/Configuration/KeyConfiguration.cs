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
        public short[] Int16 { get; set; }

        public int[] Int32 { get; set; }

        public long[] Int64 { get; set; }

        public Guid[] Guid { get; set; }

        public string[] String { get; set; }

        public DateTime[] DateTime { get; set; }

        public bool[] Boolean { get; set; }

        public DateTimeOffset[] DateTimeOffest { get; set; }

        public decimal[] Decimal { get; set; }

        public double[] Double { get; set; }

        public float[] Single { get; set; }

        public TimeSpan[] TimeSpan { get; set; }

        public byte[] Byte { get; set; }

        public byte[][] ByteArray { get; set; }

        public char[][] CharArray { get; set; }

        public KeyConfiguration()
        {
            Int16 = new short[] { 0 };
            Int32 = new[] { 0 };
            Int64 = new long[] { 0 };
            Guid = new[] { System.Guid.Empty};
            String = new[] {string.Empty};

            DateTime = new[] { System.DateTime.MinValue };
            Boolean = new[] { false };
            DateTimeOffest = new[] { DateTimeOffset.MinValue };
            Decimal = new[] { 0m };
            Double = new[] { 0d };
            Single = new[] { 0f };
            TimeSpan = new[] { System.TimeSpan.MinValue };
            Byte = new[] { System.Byte.MinValue,  };

            CharArray = new[] {  };
            ByteArray = new[] {new byte[] {1, 1}};
        }
    }
}
