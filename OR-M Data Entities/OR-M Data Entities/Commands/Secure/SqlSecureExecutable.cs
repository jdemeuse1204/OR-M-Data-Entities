/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace OR_M_Data_Entities.Commands.Secure
{
	/// <summary>
	/// Uses parameters from SqlCommand to ensure safe sql commands are passed to the database
	/// </summary>
	public abstract class SqlSecureExecutable
	{
		#region Fields
	    private readonly List<SqlSecureQueryParameter> _parameters;
		#endregion

		#region Constructor
	    protected SqlSecureExecutable()
		{
			_parameters = new List<SqlSecureQueryParameter>();
		}
		#endregion

		#region Methods
        // key where the data will be insert into the secure command
		protected string GetNextKey()
		{
			return string.Format("@DATA{0}", _parameters.Count);
		}

        public string AddParameter(string dbColumnName, object value)
        {
            var parameterKey = GetNextKey();

            _parameters.Add(new SqlSecureQueryParameter
            {
                Key = parameterKey,
                DbColumnName = dbColumnName,
                Value = new SqlSecureObject(value)
            });

            return parameterKey;
        }

        public string AddParameter(string dbColumnName, object value, SqlDbType type)
		{
            var parameterKey = GetNextKey();

            _parameters.Add(new SqlSecureQueryParameter
            {
                Key = parameterKey,
                DbColumnName = dbColumnName,
                Value = new SqlSecureObject(value, type)
            });

            return parameterKey;
		}

		protected void InsertParameters(SqlCommand cmd)
		{
			foreach (var item in _parameters)
			{
				cmd.Parameters.Add(cmd.CreateParameter()).ParameterName = item.Key;
				cmd.Parameters[item.Key].Value = item.Value.Value;

				if (item.Value.TranslateDataType)
				{
					cmd.Parameters[item.Key].SqlDbType = item.Value.DbTranslationType;
				}
			}
		}

	    public IEnumerable<SqlSecureQueryParameter> GetParameters()
	    {
	        return _parameters;
	    }

        public string FindParameterKey(string dbColumnName)
        {
            var parameter = _parameters.FirstOrDefault(w => w.DbColumnName == dbColumnName);

            return parameter != null ? parameter.Key : null;
	    }
		#endregion
	}
}
