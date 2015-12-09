using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
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
