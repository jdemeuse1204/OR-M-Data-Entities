using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    public class TestDbGenerationOptionNone
    {
        [DbGenerationOption(DbGenerationOption.None)]
        public int Id { get; set; }

        public string Test { get; set; }
    }
}
