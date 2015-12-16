using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [LookupTable("State")]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    public class State
    {
        [Key]
        public string StateCode { get; set; }

        public string StateName { get; set; }
    }
}
