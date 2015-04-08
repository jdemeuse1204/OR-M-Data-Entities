/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System.Collections.Generic;
using System.Data;
using OR_M_Data_Entities.Commands.Support;

namespace OR_M_Data_Entities.Commands.Secure.StatementParts
{
    public sealed class SqlValue : SqlFunctionTextBuilder
    {
        #region Constructor
        public SqlValue(object value, SqlDbType dataType)
        {
            Value = value;
            DataType = dataType;
        }
        #endregion

        #region Properties
        public object Value { get; set; }

        public SqlDbType DataType { get; set; }
        #endregion

        #region Methods

        public string GetValueText(Dictionary<string, object> parameters)
        {
            var result = string.Empty;
            var parameter = parameters.GetNextParameter();

            if (Value is SqlTableColumnPair)
            {
                result = ((SqlTableColumnPair)Value).GetSelectColumnText();
            }
            else
            {
                result = parameter;
                parameters.Add(parameter, Value);
            }

            foreach (var function in FunctionList)
            {
                var functionName = ((dynamic)function.Value[0]).Method.Name.ToUpper();

                switch ((string)functionName)
                {
                    case "CAST":
                        result = string.Format(" CAST({0} as {1})",
                            result,
                            function.Value[1]);
                        break;
                    case "CONVERT":
                        result = string.Format("CONVERT({0},{1},{2})",
                            function.Value[1],
                            result,
                            (function.Value[2] == null ? string.Empty : function.Value[2].ToString()));
                        break;
                }
            }

            return result;
        }
        #endregion
    }
}
