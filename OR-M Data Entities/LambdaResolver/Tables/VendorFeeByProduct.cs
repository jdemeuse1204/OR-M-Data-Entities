using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [LinkedServer("VINCADB", "OVM", "DBO")]
    public class VendorFeeByProduct
    {
        [Key]
        public int VendorProductFeeID { get; set; }
        
        public int? VendorProductID { get; set; }
        
        public int ProductTypeValue { get; set; }
        
        public string StateCode { get; set; }
        
        public int? CountyCode { get; set; }
        
        public int VendorID { get; set; }
        
        public decimal VendorFee { get; set; }

        public bool IsActive { get; set; }

        public string Comment { get; set; }

        public System.DateTime CreatedDate { get; set; }

        public System.Guid CreatedUserID { get; set; }

        public System.DateTime LastUpdatedDate { get; set; }

        public System.Guid LastUpdatedUserID { get; set; }

        public byte[] LastChanged { get; set; }

        public bool IsExcluded { get; set; }
        
        [ForeignKey("ProductTypeValue")]
        public ProductType ProductTypes { get; set; }
    }
}
