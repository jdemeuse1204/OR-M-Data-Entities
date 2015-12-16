using ORSigningPro.Common.Data.Tables.Vinca;
using ORSigningPro.Common.Infrastructure.Enum;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace ORSigningPro.Common.Data.Tables.SigningPro
{
    public class MobileClosingAddress : EntityStateTrackable
    {
        [Key]
        public int MobileClosingAddressID { get; set; }

        public string UnitNumber { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public int MobileClosingID { get; set; }

        public int CountyCode { get; set; }

        public MobileClosingAddressType MobileClosingAddressType { get; set; }

        [ForeignKey("CountyCode")]
        public County County { get; set; }

        public string Zip5 { get; set; }

        public string Zip4 { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
    }
}
