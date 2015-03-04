using System;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Mapping
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class KeyAttribute : SearchablePrimaryKeyAttribute
	{
		// SearchableKeyType needed for quick lookup in iterator
		public KeyAttribute(): base(SearchablePrimaryKeyType.Key) { }

		public override bool IsPrimaryKey
		{
			get { return true; }
		}
	}
}
