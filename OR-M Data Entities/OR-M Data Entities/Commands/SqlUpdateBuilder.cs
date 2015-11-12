/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Commands
{
    [Obsolete("Expression Query should be used instead.  If using please contact me and I will leave these in.")]
    public sealed class SqlUpdateBuilder : SqlValidation, ISqlBuilder
	{
		#region Properties
		private string _set { get; set; }
		#endregion

		#region Constructor
		public SqlUpdateBuilder()
		{
			_set = string.Empty;
		}
		#endregion

		#region Methods
		public SqlCommand Build(SqlConnection connection)
		{
			if (string.IsNullOrWhiteSpace(TableName))
			{
				throw new QueryNotValidException("UPDATE table missing");
			}

			if (string.IsNullOrWhiteSpace(_set))
			{
				throw new QueryNotValidException("UPDATE SET values missing");
			}

            var sql = string.Format("UPDATE [{0}] SET {1} {2}", TableName.TrimStart('[').TrimEnd(']'), _set.TrimEnd(','), GetValidation());
			var cmd = new SqlCommand(sql, connection);

			InsertParameters(cmd);

			return cmd;
		}

		public void AddUpdate(PropertyInfo property, object entity)
		{
            // check if its a timestamp, we need to skip the update

		    var datatype = property.GetCustomAttribute<DbTypeAttribute>();

            // never update a timestamp
		    if (datatype != null && datatype.Type == SqlDbType.Timestamp) return;

			//string fieldName, object value
			var value = property.GetValue(entity);
            var fieldName = property.GetColumnName();

			// check for sql data translation, used mostly for datetime2 inserts and updates
            var translation = property.GetCustomAttribute<DbTypeAttribute>();

			if (translation != null)
			{
				var key = AddParameter(property.GetColumnName(), value, translation.Type);

                _set += string.Format("[{0}] = {1},", fieldName, key);
			}
			else
			{
                var key = AddParameter(property.GetColumnName(), value);

                _set += string.Format("[{0}] = {1},", fieldName, key);
			}
		}

        public void AddUpdate(string column, object newValue)
        {
            //string fieldName, object value
            var data = AddParameter(column, newValue);
            _set += string.Format("[{0}] = {1},", column, data);
        }
		#endregion
	}
}
