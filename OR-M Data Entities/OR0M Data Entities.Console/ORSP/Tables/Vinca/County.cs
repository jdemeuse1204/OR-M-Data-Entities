using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    [LookupTable("County")]
    public class County : EntityStateTrackable
    {
        [Key]
        public int CountyCode { get; set; }

        public string CountyName { get; set; }
    }
}
