using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class User : EntityStateTrackable
    {
        public int ID { get; set; }

        public string Name { get; set; }
    }
}
