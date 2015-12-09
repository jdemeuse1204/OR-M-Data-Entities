using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [LinkedServer("VINCADB", "OVM", "DBO")]
    public class VendorAddress
    {
        [Key]
        public int AddressID { get; set; }

        [Key]
        public int VendorID { get; set; }

        [Key]
        public bool IsCourierAddress { get; set; }

        [ForeignKey("AddressID")]
        public virtual Address Address{ get; set; }
    }
}
