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
