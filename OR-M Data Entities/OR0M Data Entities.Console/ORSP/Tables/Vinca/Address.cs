using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    public class Address
    {
        [Key]
        public int AddressID { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string City { get; set; }

        public string StateCode { get; set; }
        
        [ForeignKey("StateCode")]
        public State State { get; set; }
        
        public string ZipCode5 { get; set; }

        public string ZipCode4 { get; set; }

        public int CountyCode { get; set; }

        [ForeignKey("CountyCode")]
        public County County { get; set; }
    }
}
