using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    [Table("ZipCode")]
    public class Zip : EntityStateTrackable
    {
        public int ID { get; set; }

        public string Zip5 { get; set; }

        public string Zip4 { get; set; }

        public int AddressID { get; set; }
    }
}
