using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [View("ActiveOrder")]
    public class MobileClosingAddress
    {
        [Key]
        public int MobileClosingAddressID { get; set; }

        public int Classification { get; set; }

        public string UnitNumber { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip5 { get; set; }

        public string Zip4 { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
    }
}
