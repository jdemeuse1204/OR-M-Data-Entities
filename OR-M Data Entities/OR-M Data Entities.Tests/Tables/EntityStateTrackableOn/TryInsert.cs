using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class TryInsert : EntityStateTrackable
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int RefId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int SomeId { get; set; }
    }
}
