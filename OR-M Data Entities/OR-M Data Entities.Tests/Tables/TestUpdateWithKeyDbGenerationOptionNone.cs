using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    public class TestUpdateWithKeyDbGenerationOptionNone
    {
        [DbGenerationOption(DbGenerationOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
