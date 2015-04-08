﻿/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Mapping.Base
{
	/// <summary>
	/// All Sql Attribute types must inherit from this class. This creates a way to 
	/// lookup all attributes instead of searching for them individually.
	/// </summary>
	public abstract class SearchablePrimaryKeyAttribute : Attribute
	{
		protected SearchablePrimaryKeyAttribute(SearchablePrimaryKeyType searchableKeyType) 
		{
			SearchableKeyType = searchableKeyType;
		}

		public SearchablePrimaryKeyType SearchableKeyType { get; private set; }

		public abstract bool IsPrimaryKey { get; }
	}
}
