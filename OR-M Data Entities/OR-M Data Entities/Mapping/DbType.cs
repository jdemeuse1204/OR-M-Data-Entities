/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Data;

namespace OR_M_Data_Entities.Mapping
{
    /// <summary>
    /// Used to denote the type in the database.  If you have a timestamp, these cannot be updated, you must mark your property as a timestamp so it is skipped.
    /// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class DbTypeAttribute : Attribute
	{
        public DbTypeAttribute(SqlDbType type, string precision = null) 
		{
			Type = type;
            Precision = precision;

		}

		public SqlDbType Type { get; set; }

        public string Precision { get; set; }
	}
}
