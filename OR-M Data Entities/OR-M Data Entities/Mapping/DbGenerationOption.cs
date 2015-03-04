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

    public enum DbGenerationOption
    {
        None,
        IdentitySpecification,
        Generate
    }
}
