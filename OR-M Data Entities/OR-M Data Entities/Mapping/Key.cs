/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Mapping
{
    /// <summary>
    /// Used to identify the primary key of a table if it is not explicitly called "Id" or "ID"
    /// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class KeyAttribute : SearchablePrimaryKeyAttribute
	{
		// SearchableKeyType needed for quick lookup in iterator
		public KeyAttribute(): base(SearchablePrimaryKeyType.Key) { }

        /// <summary>
        /// Marks the attribute as a primary key
        /// </summary>
		public override bool IsPrimaryKey
		{
			get { return true; }
		}
	}
}
