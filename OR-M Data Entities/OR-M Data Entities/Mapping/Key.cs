/*
 * OR-M Data Entities v1.1.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

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
