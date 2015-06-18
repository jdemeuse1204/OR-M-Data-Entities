/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Data;

namespace OR_M_Data_Entities.Mapping
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class DbTypeAttribute : Attribute
	{
		// SearchableKeyType needed for quick lookup in iterator
        public DbTypeAttribute(SqlDbType type) 
		{
			Type = type;
		}

		public SqlDbType Type { get; set; }
	}
}
