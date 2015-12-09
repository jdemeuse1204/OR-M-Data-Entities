using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [LinkedServer("VINCADB", "OVM", "DBO")]
    public class OrderAddress
    {
        [Key]
        public int OrderID { get; set; }

        
        public int AddressID { get; set; }

        [ForeignKey("AddressID")]
        public Address Address { get; set; }
    }
}
