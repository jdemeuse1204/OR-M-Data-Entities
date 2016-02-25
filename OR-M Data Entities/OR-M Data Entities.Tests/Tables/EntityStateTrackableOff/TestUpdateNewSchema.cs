using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff
{
    [Schema("ts")]
    public class TestUpdateNewSchema
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
