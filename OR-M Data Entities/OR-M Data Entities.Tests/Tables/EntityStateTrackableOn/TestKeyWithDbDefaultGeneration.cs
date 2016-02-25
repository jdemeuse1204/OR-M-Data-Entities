﻿using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class TestKeyWithDbDefaultGeneration : EntityStateTrackable
    {
        [DbGenerationOption(DbGenerationOption.DbDefault)]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
