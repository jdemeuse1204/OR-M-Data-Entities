using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    public class VendorAddress
    {
        [Key]
        public int AddressID { get; set; }

        [Key]
        public int VendorID { get; set; }

        [Key]
        public bool IsCourierAddress { get; set; }

        [ForeignKey("AddressID")]
        public Address Address{ get; set; }
    }
}
