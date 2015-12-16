using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    public class VendorProduct
    {
        [Key]
        public int VendorProductID { get; set; }

        public string VendorProductName { get; set; }

        public bool IsActive { get; set; }

        
    }
}
