using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables
{
    [View("ContactAndPhone")]
    [Table("PhoneNumbers")]
    public class PhoneNumber : EntityStateTrackable
    {
        public int ID { get; set; }

        public string Phone{ get; set; }

        public int PhoneTypeID { get; set; }

        [ForeignKey("PhoneTypeID")]
        public PhoneType PhoneType { get; set; }
    }
}
