﻿using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    [Schema("ts")]
    public class TestUpdateNewSchema : EntityStateTrackable
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}