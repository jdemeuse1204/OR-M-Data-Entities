﻿/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data;

namespace OR_M_Data_Entities.Data.Query
{
    /// <summary>
    /// Used for casting and converting columns in the select statement part of a query.  Only used when reading data
    /// </summary>
    public abstract class SqlFunctionTextBuilder
    {
        #region Constructor
        protected SqlFunctionTextBuilder()
        {
            _functionList = new Dictionary<int, object[]>();
        }
        #endregion

        #region Properties
        private readonly Dictionary<int, object[]> _functionList;
        public IEnumerable<KeyValuePair<int, object[]>> FunctionList
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
        #endregion
    }
}