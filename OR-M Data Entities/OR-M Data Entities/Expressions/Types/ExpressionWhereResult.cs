/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using OR_M_Data_Entities.Commands;

namespace OR_M_Data_Entities.Expressions.Types
{
    public sealed class ExpressionWhereResult : ExpressionSelectResult
    {
        private object _compareValue;
        public object CompareValue
        {
            get { return _compareValue; }
            set { _compareValue = (value ?? DBNull.Value); }
        }

        public ComparisonType ComparisonType { get; set; }
    }
}
