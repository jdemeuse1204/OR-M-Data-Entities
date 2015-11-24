/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Query.StatementParts;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Data.Query
{
	/// <summary>
	/// Builds the WHERE statement for queries
	/// </summary>
	public abstract class SqlValidation : SqlFromTable
    {
		#region Properties
		private string _where { get; set; }
		#endregion

		#region Constructor
	    protected SqlValidation(ConfigurationOptions configuration)
            : base(configuration)
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

		public void AddWhere(string field, CompareType type, object equals)
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

            var data = AddParameter(field, equals);
			_where += string.Format(startValidationString, 
				_getValidationType(), 
				string.IsNullOrWhiteSpace(TableName) ? "" : string.Format("[{0}].", FormattedTableName), 
				field, 
				comparisonType, 
				startComparisonType, 
				data, 
				endComparisonType);

			;
		}
		#endregion
	}
}
