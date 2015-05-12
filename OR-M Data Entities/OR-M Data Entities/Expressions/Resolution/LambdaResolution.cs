﻿using System;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class LambdaResolution
    {
        public LambdaResolution()
        {
            TableName = string.Empty;
            ColumnName = string.Empty;
            Comparison = CompareType.None;
            Group = -1;
        }

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public CompareType Comparison { get; set; }

        public object CompareValue { get; set; }

        public int Group { get; set; }

        public string GetComparisonStringOperator()
        {
            switch (Comparison)
            {
                case CompareType.BeginsWith:
                case CompareType.Like:
                case CompareType.EndsWith:
                    return "LIKE";

                case CompareType.None:
                    throw new Exception("Comparison cannot be None");

                case CompareType.NotLike:
                case CompareType.NotBeginsWith:
                case CompareType.NotEndsWith:
                    return "NOT LIKE";
                
                case CompareType.Equals:
                    return "=";
                case CompareType. GreaterThan:
                    return ">";
                case CompareType.GreaterThanEquals:
                    return ">=";
                case CompareType.LessThan:
                    return "<";
                case CompareType.LessThanEquals:
                    return "<=";
                case CompareType.NotEqual:
                    return "!=";
                case CompareType.Between:
                    return "BETWEEN";
                case CompareType.In:
                    return "IN";
                case CompareType.NotIn:
                    return "NOT IN";
                default:
                    return string.Empty;
            }
        }
    }
}
