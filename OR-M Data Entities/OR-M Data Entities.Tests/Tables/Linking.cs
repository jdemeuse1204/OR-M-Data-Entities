using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    public class Linking
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
