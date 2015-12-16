using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    public class VendorRanking
    {
        [Key]
        public int RankingID { get; set; }

        public int VendorID { get; set; }

        public decimal OverallRanking { get; set; }

        public int VendorProductID { get; set; }

        public int CountyCode { get; set; }

        public string StateCode { get; set; }
    }
}
