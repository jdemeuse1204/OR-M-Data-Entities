/*
 * OR-M Data Entities v1.0.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Data;

namespace OR_M_Data_Entities.Expressions.Types
{
    public class ExpressionSelectResult
    {
        public string ColumnName { get; set; }

        public Type ColumnType { get; set; }

        public string TableName { get; set; }

        public SqlDbType Transform { get; set; }

        public bool ShouldCast { get; set; }

		public bool ShouldConvert { get; set; }

		public int ConversionStyle { get; set; }
    }
}
