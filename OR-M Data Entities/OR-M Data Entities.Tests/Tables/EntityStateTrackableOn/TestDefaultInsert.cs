using System;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class TestDefaultInsert : EntityStateTrackable
    {
        public int Id { get; set; }

        [DbGenerationOption(DbGenerationOption.DbDefault)]
        public Guid Uid { get; set; }

        public string Name { get; set; }
    }
}
