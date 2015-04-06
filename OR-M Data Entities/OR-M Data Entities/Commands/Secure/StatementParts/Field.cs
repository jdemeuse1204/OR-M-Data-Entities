/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

namespace OR_M_Data_Entities.Commands.Support
{
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
