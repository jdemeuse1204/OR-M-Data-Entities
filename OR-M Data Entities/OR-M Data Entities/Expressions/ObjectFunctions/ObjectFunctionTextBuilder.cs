/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace OR_M_Data_Entities.Expressions.ObjectFunctions
{
    public abstract class ObjectFunctionTextBuilder
    {
        #region Constructor
        protected ObjectFunctionTextBuilder()
        {
            _functionList = new Dictionary<int, object[]>();
        }
        #endregion

        #region Properties
        private readonly Dictionary<int, object[]> _functionList;
        public IReadOnlyDictionary<int, object[]> FunctionList
        {
            get { return _functionList; }
        }
        #endregion

        #region Methods
        public void AddFunction(Func<object, SqlDbType, object> function, SqlDbType dataType)
        {
            _functionList.Add(_functionList.Count + 1, new object[] { function, dataType });
        }

        public void AddFunction(Func<SqlDbType, object, int?, object> function, SqlDbType dataType, int? style)
        {
            _functionList.Add(_functionList.Count + 1, new object[] { function, dataType, style });
        }

        protected string GetFunctionText(object[] function,string tableAndColumnText)
        {
            var functionName = (function[0] as dynamic).Method.Name;

            if (functionName.ToUpper() == "CAST")
            {
                // cast
                return string.Format("CAST({0} AS {1})", tableAndColumnText, function[1]);
            }
           
            // convert
            var style = (int)function[2];

            return string.Format("CONVERT({0},{1}{2})", function[1], tableAndColumnText,
                style == 0 ? string.Empty : style.ToString(CultureInfo.InvariantCulture));
        }
        #endregion
    }
}
