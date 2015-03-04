using System;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Mapping
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ColumnAttribute : SearchablePrimaryKeyAttribute
	{
		// SearchableKeyType needed for quick lookup in iterator
		public ColumnAttribute(string name) : base(SearchablePrimaryKeyType.Column)
		{
			Name = name;
		}

		public string Name { get; private set; }

		public override bool IsPrimaryKey
		{
			get { return Name.ToUpper() == "ID"; }
		}
	}
}
