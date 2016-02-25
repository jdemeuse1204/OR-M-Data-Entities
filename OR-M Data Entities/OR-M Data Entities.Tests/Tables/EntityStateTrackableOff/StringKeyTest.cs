using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff
{
    public class StringKeyTest
    {
        [DbGenerationOption(DbGenerationOption.None)]
        public string Id { get; set; }

        public string Value { get; set; }
    }
}
