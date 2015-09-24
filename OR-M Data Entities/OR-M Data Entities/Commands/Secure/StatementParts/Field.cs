/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

namespace OR_M_Data_Entities.Commands.Secure.StatementParts
{
    /// <summary>
    /// Used with ISqlBuilder if you want to select a field with sql and give it an alias
    /// </summary>
	public sealed class Field
	{
		public string ColumnName { get; set; }
		public string Alias { get; set; }

		public Field(string columnName, string alias = "")
		{
			ColumnName = columnName;
			Alias = alias;
		}
	}
}
