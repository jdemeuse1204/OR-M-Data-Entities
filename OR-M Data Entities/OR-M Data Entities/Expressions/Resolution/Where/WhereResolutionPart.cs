/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Transform;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Base;

namespace OR_M_Data_Entities.Expressions.Resolution.Where
{
    public class WhereResolutionPart : IQueryPart
    {
        public WhereResolutionPart()
        {
            TableName = string.Empty;
            ColumnName = string.Empty;
            Comparison = CompareType.None;
            Group = -1;
            Transform = new TransformContainer();
        }

        public Guid ExpressionQueryId { get; set; }

        public string TableName { get; set; }

        public string TableAlias { get; set; }

        public string ColumnName { get; set; }

        public CompareType Comparison { get; set; }

        public object CompareValue { get; set; }

        public bool InvertComparison { get; set; }

        public readonly TransformContainer Transform;

        public int Group { get; set; }

        public string GetComparisonStringOperator()
        {
            if (CompareValue is SqlDbParameter && ((SqlDbParameter) CompareValue).Value.Equals("IS NULL")) return string.Empty;

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
