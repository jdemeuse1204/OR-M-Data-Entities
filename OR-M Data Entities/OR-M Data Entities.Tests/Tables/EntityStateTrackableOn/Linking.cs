using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class Linking : EntityStateTrackable
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int PolicyId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int PolicyInfoId { get; set; }

        public string Description { get; set; }
    }
}
