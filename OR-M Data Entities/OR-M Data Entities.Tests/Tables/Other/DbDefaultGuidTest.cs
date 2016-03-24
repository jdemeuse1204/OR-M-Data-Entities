using System;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables.Other
{
    public class DbDefaultGuidTest
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.DbDefault)]
        public Guid Id { get; set; }

        public string Test { get; set; }
    }
}
