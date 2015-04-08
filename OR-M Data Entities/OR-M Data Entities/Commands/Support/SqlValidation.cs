/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using OR_M_Data_Entities.Commands.Secure.StatementParts;

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

		public void AddWhere(string table, string field, ComparisonType type, object equals)
		{
			var comparisonType = "=";
			var startComparisonType = "";
			var endComparisonType = "";
			var startValidationString = " {0} [{1}].[{2}] {3} {4}{5}{6} ";

			switch (type)
			{
				case ComparisonType.Contains:
					startComparisonType = "'%";
					endComparisonType = "%'";
					comparisonType = "LIKE";
					break;
				case ComparisonType.BeginsWith:
					endComparisonType = "%'";
					comparisonType = "LIKE";
					break;
				case ComparisonType.EndsWith:
					startComparisonType = "'%";
					comparisonType = "LIKE";
					break;
				case ComparisonType.EqualsIgnoreCase:  // not used
					startValidationString = " {0} [{1}].[{2}] {3} {4}{5}{6} ";
					break;
				case ComparisonType.EqualsTruncateTime: // not used
					startValidationString = " {0} Cast([{1}].[{2}] as date) {3} Cast({4}{5}{6} as date)";
					break;
				case ComparisonType.GreaterThan:
					comparisonType = ">";
					break;
				case ComparisonType.GreaterThanEquals:
					comparisonType = ">=";
					break;
				case ComparisonType.LessThan:
					comparisonType = "<";
					break;
				case ComparisonType.LessThanEquals:
					comparisonType = "<=";
					break;
				case ComparisonType.NotEqual:
					comparisonType = "!=";
					break;
			}

			var data = GetNextParameter();
			_where += string.Format(startValidationString, 
				_getValidationType(), 
				table, 
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
