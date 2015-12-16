using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    public class OrderContact
    {
        [Key]
        public int ContactID { get; set; }

        public int OrderID { get; set; }

        [ForeignKey("ContactID")]
        public Contact Contact { get; set; }
    }
}
