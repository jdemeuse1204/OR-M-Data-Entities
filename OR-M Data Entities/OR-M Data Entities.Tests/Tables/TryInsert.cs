using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    public class TryInsert
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int RefId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int SomeId { get; set; }
    }
}
