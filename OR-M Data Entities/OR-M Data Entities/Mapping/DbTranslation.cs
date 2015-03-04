using System;
using System.Data;

namespace OR_M_Data_Entities.Mapping
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class DbTranslationAttribute : Attribute
	{
		// SearchableKeyType needed for quick lookup in iterator
		public DbTranslationAttribute(SqlDbType type) 
		{
			Type = type;
		}

		public SqlDbType Type { get; set; }
	}
}
