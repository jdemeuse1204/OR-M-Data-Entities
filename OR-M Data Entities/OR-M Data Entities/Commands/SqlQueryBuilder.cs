/*
 * OR-M Data Entities v1.0.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands
{
    public sealed class SqlQueryBuilder : SqlValidation, ISqlBuilder
	{
		#region Properties
		private string _select { get; set; }
		private string _columns { get; set; }
		private string _from { get; set; }
		private string _table { get; set; }
        private string _straightSelect { get; set; }
		private Dictionary<string, object> _parameters { get; set; }
		#endregion

		#region Constructor
		public SqlQueryBuilder()
		{
			_select = string.Empty;
			_columns = string.Empty;
			_from = string.Empty;
			_table = string.Empty;
		    _straightSelect = string.Empty;
			_parameters = new Dictionary<string, object>();
		}
		#endregion

		#region Methods
		public SqlCommand Build(SqlConnection connection)
		{
            if (string.IsNullOrWhiteSpace(_select) && string.IsNullOrWhiteSpace(_straightSelect))
			{
				throw new QueryNotValidException("SELECT statement missing");
			}

            if (string.IsNullOrWhiteSpace(_from) && string.IsNullOrWhiteSpace(_straightSelect))
			{
				throw new QueryNotValidException("FROM statement missing");
			}

			var sql = string.IsNullOrWhiteSpace(_straightSelect) ? 
                _select + _columns.TrimEnd(',') + _from + GetValidation() : 
                _straightSelect;

			var cmd = new SqlCommand(sql, connection);

			InsertParameters(cmd);

			return cmd;
		}

		public void Table(string tableName)
		{
			_from += string.Format(" FROM {0} ", tableName);
		}

		public void Select(string table, params Field[] fields)
		{
			_select = " SELECT ";

			foreach (var field in fields)
			{
				var alias = string.IsNullOrWhiteSpace(field.Alias) ? "" : string.Format(" AS {0}", field.Alias);
				_columns += string.Format("[{0}].[{1}]{2},", table, field.ColumnName, alias);
			}
		}

		public void SelectAll()
		{
			_select = " SELECT * ";
		}

		public void SelectAll<T>()
		{
		    var tableName = DatabaseSchemata.GetTableName(Activator.CreateInstance<T>());

            Table(tableName);

			_select = " SELECT * ";
		}

		public void SelectTopOneAll()
		{
			_select = " SELECT TOP 1 * ";
		}

        public void SelectTopOneAll<T>()
        {
            var tableName = DatabaseSchemata.GetTableName(Activator.CreateInstance<T>());

            Table(tableName);

            _select = " SELECT TOP 1 * ";
        }

		public void SelectTop(int rows, string table, params Field[] fields)
		{
			_select = string.Format(" SELECT TOP {0} ", rows);

			foreach (var field in fields)
			{
				var alias = string.IsNullOrWhiteSpace(field.Alias) ? "" : string.Format(" AS {0}", field.Alias);
				_columns += string.Format("[{0}].[{1}]{2},", table, field.ColumnName, alias);
			}
		}

	    public void Select(string sql)
	    {
	        _straightSelect = sql;
	    }

		public void AddJoin(JoinType type, string parentTable, string parentField, string childTable, string childField)
		{
			switch (type)
			{
				case JoinType.Equi:
					_from += string.Format(",{0}", childTable);
					AddWhere(parentTable, parentField, childTable, childField);
					break;
				case JoinType.Inner:
					_from += string.Format(" INNER JOIN [{0}] On [{1}].[{2}] = [{3}].[{4}] ",
						parentTable,
						parentTable,
						parentField,
						childTable,
						childField);
					break;
				case JoinType.Left:
					_from += string.Format(" LEFT JOIN [{0}] On [{1}].[{2}] = [{3}].[{4}] ",
						parentTable,
						parentTable,
						parentField,
						childTable,
						childField);
					break;
			}
		}
		#endregion
	}
}
