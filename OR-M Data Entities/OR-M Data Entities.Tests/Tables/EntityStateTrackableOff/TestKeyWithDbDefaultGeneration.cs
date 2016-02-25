using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff
{
    public class TestKeyWithDbDefaultGeneration
    {
        [DbGenerationOption(DbGenerationOption.DbDefault)]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
