/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Mapping
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class TableAttribute : Attribute
	{
		public TableAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
	}
}
