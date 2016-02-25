using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff
{
    [Schema("ts")]
    [Table("TestUpdateWithKeyDbGenerationOptionNone")]
    public class TestUpdateWithKeyDbGenerationOptionNone_ts
    {
        [DbGenerationOption(DbGenerationOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
