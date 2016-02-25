using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class RecordLabel : EntityStateTrackable
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
