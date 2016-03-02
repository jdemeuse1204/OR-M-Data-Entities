/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Mapping
{
    /// <summary>
    /// This attribute should be used to specify how a primary key is generated.  When set to Identity Specification, the ORM relies on the 
    /// database to create the key.  When set to Generate, the ORM will create the key for you.  If set to None, then no key will be generated.
    /// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
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
        Generate,
        DbDefault
    }
}
