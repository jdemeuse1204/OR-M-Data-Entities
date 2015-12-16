using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    public class VendorOrderStatus
    {
        [Key]
        public int ItemValue { get; set; }
        
        public string ItemName { get; set; }
        
        public int ProductTypeValue { get; set; }
        
        public int? OrderBy { get; set; }
        
        [ForeignKey("ProductTypeValue")]
        public ProductType ProductTypes { get; set; }
    }
}
