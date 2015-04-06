/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using System;
using System.Collections;
using System.Collections.Generic;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.StatementParts
{
    public sealed class SqlWhere
    {
        public SqlTableColumnPair TableCompareValue { get; set; }

        private object _objectCompareValue;
        public object ObjectCompareValue
        {
            get { return _objectCompareValue; }
            set { _objectCompareValue = (value ?? DBNull.Value); }
        }

        public ComparisonType ComparisonType { get; set; }

        public string GetWhereText(Dictionary<string, object> parameters)
        {
            var nextParameter = parameters.GetNextParameter();
            var compareString = string.Empty;

            if (ObjectCompareValue is SqlTableColumnPair)
            {
                compareString = ((SqlTableColumnPair)ObjectCompareValue).GetSelectColumnText();
            }
            else if (ObjectCompareValue == DBNull.Value)
            {
                compareString = "IS NULL";
            }
            else if (ObjectCompareValue is SqlValue)
            {
                var sqlValue = (SqlValue) ObjectCompareValue;

                compareString = sqlValue.GetValueText(parameters);
            }
            else if (ObjectCompareValue.IsList())
            {
                compareString = string.Format("({0})", 
                    _enumerateList(ObjectCompareValue as dynamic, parameters));
            }
            else
            {
                var resolvedObjectCompareValue = _resolveObject();
                parameters.Add(nextParameter, resolvedObjectCompareValue);
                compareString = nextParameter;
            }

            return string.Format("{0} {1} {2}",
                TableCompareValue.GetSelectColumnText(),
                DatabaseOperations.GetComparisonString(this),
                compareString);
        }

        private string _enumerateList(IEnumerable list, Dictionary<string, object> parameters)
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

        private object _resolveObject()
        {
            if (ComparisonType == ComparisonType.Contains
                && ObjectCompareValue.IsList())
            {
                return ObjectCompareValue;
            }

            switch (ComparisonType)
            {
                case ComparisonType.Contains:
                    return "%" + Convert.ToString(ObjectCompareValue) + "%";
                case ComparisonType.BeginsWith:
                    return Convert.ToString(ObjectCompareValue) + "%";
                case ComparisonType.EndsWith:
                    return "%" + Convert.ToString(ObjectCompareValue);
                default:
                    return ObjectCompareValue;
            }
        }
    }
}
