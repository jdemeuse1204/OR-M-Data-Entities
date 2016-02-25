using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff
{
    [Table("TryInsertWithGeneration")]
    public class TryInsertUpdateWithGeneration 
    {
        [DbGenerationOption(DbGenerationOption.None)]
        public int Id { get; set; }

        [DbGenerationOption(DbGenerationOption.Generate)]
        public int SequenceNumber { get; set; }

        [DbGenerationOption(DbGenerationOption.IdentitySpecification)]
        public int OtherNumber { get; set; }

        public string Name { get; set; }
    }
}
