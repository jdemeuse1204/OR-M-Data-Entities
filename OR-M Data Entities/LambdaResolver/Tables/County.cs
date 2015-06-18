using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [LinkedServer("VINCADB", "OVM", "DBO")]
    public class County
    {
        [Key]
        public int CountyCode { get; set; }

        public string CountyName { get; set; }
    }
}
