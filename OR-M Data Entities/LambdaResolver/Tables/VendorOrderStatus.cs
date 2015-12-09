using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [LinkedServer("VINCADB", "OVM", "DBO")]
    public class VendorOrderStatus
    {
        [Key]
        public int ItemValue { get; set; }
        
        public string ItemName { get; set; }
        
        public int ProductTypeValue { get; set; }
        
        public int? OrderBy { get; set; }

        //public virtual List<AncillaryProduct_Vendor> AncillaryProduct_Vendors { get; set; }
        
        [ForeignKey("ProductTypeValue")]
        public ProductType ProductTypes { get; set; }
    }
}
