using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class TestDbGenerationOptionNone : EntityStateTrackable
    {
        [DbGenerationOption(DbGenerationOption.None)]
        public int Id { get; set; }

        public string Test { get; set; }
    }
}
