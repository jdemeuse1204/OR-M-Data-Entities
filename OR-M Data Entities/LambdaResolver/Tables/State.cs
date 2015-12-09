using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [LinkedServer("VINCADB", "OVM", "DBO")]
    public class State
    {
        [Key]
        public string StateCode { get; set; }

        public string StateName { get; set; }
    }
}
