using System;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("PolicyInfo")]
    public class PolicyInfo_READONLY : EntityStateTrackable
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Description { get; set; }

        public Guid Stamp { get; set; }
    }
}
