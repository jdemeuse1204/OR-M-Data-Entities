using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    public class OrderAddress
    {
        [Key]
        public int OrderID { get; set; }

        
        public int AddressID { get; set; }

        [ForeignKey("AddressID")]
        public Address Address { get; set; }
    }
}
