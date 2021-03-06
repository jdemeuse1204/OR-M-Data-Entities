﻿using System.Data;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    [LookupTable("History")]
    public class History : EntityStateTrackable
    {
        public int Id { get; set; }

        [DbType(SqlDbType.Timestamp)]
        public byte[] CreateDate { get; set; }

        public string Description { get; set; }

        public int ComputerId { get; set; }
    }
}
