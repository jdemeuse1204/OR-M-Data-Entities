using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables
{
    public class PhoneType : EntityStateTrackable
    {
        public int ID { get; set; }

        public string Type { get; set; }
    }
}
