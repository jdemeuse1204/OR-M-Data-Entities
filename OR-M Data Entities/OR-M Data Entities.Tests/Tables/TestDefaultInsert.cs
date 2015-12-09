using System;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    public class TestDefaultInsert
    {
        public int Id { get; set; }

        [DbGenerationOption(DbGenerationOption.DbDefault)]
        public Guid Uid { get; set; }

        public string Name { get; set; }
    }
}
