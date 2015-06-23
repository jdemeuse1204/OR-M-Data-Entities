/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Mapping
{
    /// <summary>
    /// Any Table marked with the ReadOnly Attribute will be come readonly and can never be modified in the database.
    /// Will throw an error if a save is attempted and ThrowErrorOnSave = true. 
    /// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class ReadOnlyAttribute : Attribute
	{

	}
}
