/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Mapping
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class DbGenerationOptionAttribute : Attribute
	{
        public DbGenerationOptionAttribute(DbGenerationOption option)
		{
			Option = option;
		}

        public DbGenerationOption Option { get; private set; }
	}

    /// <summary>
    /// Specifies the columns value generation
    /// Note:  If a primary key is set to none then a record will always be inserted
    /// </summary>
    public enum DbGenerationOption
    {
        None,
        IdentitySpecification,
        Generate
    }
}
