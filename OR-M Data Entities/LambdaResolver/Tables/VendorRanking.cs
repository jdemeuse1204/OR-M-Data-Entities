using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [LinkedServer("VINCADB", "OVM", "DBO")]
    public class VendorRanking
    {
        [Key]
        public int RankingID { get; set; }

        public int VendorID { get; set; }

        public decimal OverallRanking { get; set; }
    }
}
