using System;
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
            const string result = "[{0}].[{1}] {2} {3}";
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

                parameters.Add(nextParameter, sqlValue.Value);
                compareString = sqlValue.GetValueText(parameters);
            }
            else
            {
                parameters.Add(nextParameter, ObjectCompareValue);
                compareString = nextParameter;
            }

            return string.Format(result,
                TableCompareValue.GetTableName(),
                TableCompareValue.GetColumnName(),
                DatabaseOperations.GetComparisonString(this),
                compareString);
        }
    }
}
