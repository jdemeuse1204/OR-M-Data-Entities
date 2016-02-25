using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class Name : EntityStateTrackable
    {
        public int ID { get; set; }

        public string Value { get; set; }

        public int ContactID { get; set; }
    }
}
