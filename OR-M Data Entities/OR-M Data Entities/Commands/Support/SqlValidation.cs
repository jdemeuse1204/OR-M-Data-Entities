/*
 * OR-M Data Entities v2.2
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using OR_M_Data_Entities.Commands.Secure.StatementParts;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Commands.Support
{
	/// <summary>
	/// Builds the WHERE statement for queries
	/// </summary>
	public abstract class SqlValidation : SqlFromTable
	{
		#region Properties
		private string _where { get; set; }
		//private const string COMPARECASE = "COLLATE SQL_Latin1_General_CP1_CS_AS";
		//private const string IGNORECASE = "COLLATE SQL_Latin1_General_CP1_CI_AS";
		#endregion

		#region Constructor
	    protected SqlValidation()
		{
			_where = string.Empty;
		}
		#endregion

		#region Methods
		public string GetValidation()
		{
			return _where;
		}

		private string _getValidationType()
		{
			return _where.Contains("WHERE") ? "AND " : "WHERE ";
		}

		public void AddWhere(string parentTable, string parentField, string childTable, string childField)
		{
			_where += string.Format(" {0} [{1}].[{2}] = [{3}].[{4}] ",
						_getValidationType(),
						parentTable,
						parentField,
						childTable,
						childField);
		}

		public void AddWhere(string table, string field, CompareType type, object equals)
		{
			var comparisonType = "=";
			var startComparisonType = "";
			var endComparisonType = "";
			var startValidationString = " {0} {1}[{2}] {3} {4}{5}{6} ";

			switch (type)
			{
				case CompareType.Like:
					startComparisonType = "'%";
					endComparisonType = "%'";
					comparisonType = "LIKE";
					break;
				case CompareType.BeginsWith:
					endComparisonType = "%'";
					comparisonType = "LIKE";
					break;
				case CompareType.EndsWith:
					startComparisonType = "'%";
					comparisonType = "LIKE";
					break;
				case CompareType.GreaterThan:
					comparisonType = ">";
					break;
				case CompareType.GreaterThanEquals:
					comparisonType = ">=";
					break;
				case CompareType.LessThan:
					comparisonType = "<";
					break;
				case CompareType.LessThanEquals:
					comparisonType = "<=";
					break;
				case CompareType.NotEqual:
					comparisonType = "!=";
					break;
			}

			var data = GetNextParameter();
			_where += string.Format(startValidationString, 
				_getValidationType(), 
				string.IsNullOrWhiteSpace(table) ? "" : string.Format("[{0}].", table), 
				field, 
				comparisonType, 
				startComparisonType, 
				data, 
				endComparisonType);

			AddParameter(equals);
		}
		#endregion
	}
}
