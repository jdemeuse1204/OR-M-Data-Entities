/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Data;

namespace OR_M_Data_Entities.Data.Secure
{
    /// <summary>
    /// Ensures that an object is used with query builders in a secure manner (used as a query parameter).  
    /// Also has translation type for translating values to sql types.
    /// </summary>
	public sealed class SqlSecureObject
    {
        #region Constructor
        public SqlSecureObject(object value)
		{
			TranslateDataType = false;
			_value = value;
		}

		public SqlSecureObject(object value, SqlDbType type)
		{
			TranslateDataType = true;
			DbTranslationType = type;
			_value = value;
		}
        #endregion

        #region Properties
        public bool TranslateDataType { get; private set; }

		public SqlDbType DbTranslationType { get; set; }

		private object _value { get; set; }
		public object Value 
		{
			get { return (_value ?? DBNull.Value); }
			set { _value = value; }
        }
        #endregion
    }
}
