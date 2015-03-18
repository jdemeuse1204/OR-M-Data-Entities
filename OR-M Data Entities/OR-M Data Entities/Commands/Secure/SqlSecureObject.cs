/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Data;

namespace OR_M_Data_Entities.Commands.Secure
{
	public sealed class SqlSecureObject
	{
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

		public bool TranslateDataType { get; private set; }

		public SqlDbType DbTranslationType { get; set; }

		private object _value { get; set; }
		public object Value 
		{
			get { return (_value ?? DBNull.Value); }
			set { _value = value; }
		}
	}
}
