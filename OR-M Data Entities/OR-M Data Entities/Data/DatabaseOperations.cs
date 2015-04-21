/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.Secure.StatementParts;
using OR_M_Data_Entities.Commands.Transform;
using OR_M_Data_Entities.Expressions.ObjectMapping;

namespace OR_M_Data_Entities.Data
{
    public static class DatabaseOperations
    {
        #region Casting and Converting

        public static bool IsCasting(object expression)
        {
            return _isTransform(expression, "Cast");
        }

        public static bool IsConverting(object expression)
        {
            return _isTransform(expression, "Convert");
        }

        private static bool _isTransform(object expression, string methodName)
        {
            if (!(expression is MethodCallExpression)) return false;

            var methodCallExpression = (MethodCallExpression)expression;

            return methodCallExpression.Method.DeclaringType == typeof(DbFunctions) &&
                   String.Equals(methodCallExpression.Method.Name, methodName, StringComparison.CurrentCultureIgnoreCase);
        }

        public static string GetComparisonStringWithFormatValues(SqlWhere where)
        {
            if (where.ComparisonType == ComparisonType.Contains)
            {
                return where.ObjectCompareValue.IsList() ? " {0} IN ({1}) " : " {0} LIKE {1}";
            }

            switch (where.ComparisonType)
            {
                case ComparisonType.BeginsWith:
                case ComparisonType.EndsWith:
                    return " {0} LIKE {1}";
                case ComparisonType.Equals:
                    return "{0} = {1}";
                case ComparisonType.EqualsIgnoreCase:
                    return "";
                case ComparisonType.EqualsTruncateTime:
                    return "";
                case ComparisonType.GreaterThan:
                    return "{0} > {1}";
                case ComparisonType.GreaterThanEquals:
                    return "{0} >= {1}";
                case ComparisonType.LessThan:
                    return "{0} < {1}";
                case ComparisonType.LessThanEquals:
                    return "{0} <= {1}";
                case ComparisonType.NotEqual:
                    return "{0} != {1}";
                default:
                    throw new ArgumentOutOfRangeException("comparison");
            }
        }

        public static string GetComparisonString(SqlWhere where)
        {
            if (where.ComparisonType == ComparisonType.Contains)
            {
                return where.ObjectCompareValue.IsList() ? "IN" : "LIKE";
            }

            switch (where.ComparisonType)
            {
                case ComparisonType.BeginsWith:
                case ComparisonType.EndsWith:
                    return "LIKE";
                case ComparisonType.Equals:
                    return "=";
                case ComparisonType.EqualsIgnoreCase:
                    return "";
                case ComparisonType.EqualsTruncateTime:
                    return "";
                case ComparisonType.GreaterThan:
                    return ">";
                case ComparisonType.GreaterThanEquals:
                    return ">=";
                case ComparisonType.LessThan:
                    return "<";
                case ComparisonType.LessThanEquals:
                    return "<=";
                case ComparisonType.NotEqual:
                    return "!=";
                default:
                    throw new ArgumentOutOfRangeException("comparison");
            }
        }

        public static string EnumerateList(IEnumerable list, Dictionary<string, object> parameters)
        {
            var result = "";

            foreach (var item in list)
            {
                var parameter = parameters.GetNextParameter();
                parameters.Add(parameter, item);

                result += parameter + ",";
            }

            return result.TrimEnd(',');
        }

        private static string _addParameter(object value, Dictionary<string, object> parameters)
        {
            var parameter = parameters.GetNextParameter();
            parameters.Add(parameter, value);

            return parameter;
        }

        public static string GetComparisonString(ObjectColumn objectColumn, object compareValue, ComparisonType comparisonType, Dictionary<string,object> parameters)
        {
            var isCompareValueList = compareValue.IsList();

            if (comparisonType == ComparisonType.Contains)
            {
                return string.Format(isCompareValueList ? " {0} IN ({1}) " : " {0} LIKE {1}", objectColumn.GetText(),
                    isCompareValueList
                        ? EnumerateList(compareValue as IEnumerable, parameters)
                        : _addParameter(compareValue, parameters));
            }

            switch (comparisonType)
            {
                case ComparisonType.BeginsWith:
                case ComparisonType.EndsWith:
                    return string.Format(" {0} LIKE {1}", objectColumn.GetText(),
                        _addParameter(compareValue, parameters));
                case ComparisonType.Equals:
                    return string.Format(" {0} = {1}", objectColumn.GetText(),
                        _addParameter(compareValue, parameters));
                case ComparisonType.EqualsIgnoreCase:
                    return "";
                case ComparisonType.EqualsTruncateTime:
                    return "";
                case ComparisonType.GreaterThan:
                    return string.Format(" {0} > {1}", objectColumn.GetText(),
                        _addParameter(compareValue, parameters));
                case ComparisonType.GreaterThanEquals:
                    return string.Format(" {0} >= {1}", objectColumn.GetText(),
                        _addParameter(compareValue, parameters));
                case ComparisonType.LessThan:
                    return string.Format(" {0} < {1}", objectColumn.GetText(),
                         _addParameter(compareValue, parameters));
                case ComparisonType.LessThanEquals:
                    return string.Format(" {0} <= {1}", objectColumn.GetText(),
                        _addParameter(compareValue, parameters));
                case ComparisonType.NotEqual:
                    return string.Format(" {0} != {1}", objectColumn.GetText(),
                        _addParameter(compareValue, parameters));
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Cannot resolve comparison type {0}",
                        comparisonType));
            }
        }
        #endregion
    }
}
