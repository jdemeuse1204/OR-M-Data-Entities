/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Commands
{
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
		public SqlCommand Build(SqlConnection connection, out DataQueryType dataQueryType)
		{
            dataQueryType = DataQueryType.NoForeignKeys;

			if (string.IsNullOrWhiteSpace(TableName))
			{
				throw new QueryNotValidException("UPDATE table missing");
			}

			if (string.IsNullOrWhiteSpace(_set))
			{
				throw new QueryNotValidException("UPDATE SET values missing");
			}

            var sql = string.Format("UPDATE {0} SET {1} {2}", TableName, _set.TrimEnd(','), GetValidation());
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
            var fieldName = DatabaseSchemata.GetColumnName(property);
			var data = GetNextParameter();
			_set += string.Format("[{0}] = {1},", fieldName, data);

			// check for sql data translation, used mostly for datetime2 inserts and updates
            var translation = property.GetCustomAttribute<DbTypeAttribute>();

			if (translation != null)
			{
				AddParameter(value, translation.Type);
			}
			else
			{
				AddParameter(value);
			}
		}

        public void AddUpdate(string column, object newValue)
        {
            //string fieldName, object value
            var data = GetNextParameter();
            _set += string.Format("[{0}] = {1},", column, data);

           AddParameter(newValue);
        }
		#endregion
	}
}
