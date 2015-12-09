using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [LinkedServer("VINCADB", "OVM", "DBO")]
    public class ProductType
    {
        [Key]
        public int ItemValue { get; set; }
        
        public string ItemName { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public string AccountNumber { get; set; }
    }
}
