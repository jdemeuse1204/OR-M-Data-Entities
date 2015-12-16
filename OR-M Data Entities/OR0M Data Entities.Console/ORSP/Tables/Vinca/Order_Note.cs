using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    // ReSharper disable once InconsistentNaming
    public class Order_Note
    {
        [Key]
        public int NoteID { get; set; }

        public int OrderID { get; set; }

        public int NoteTypeValue { get; set; }

        public string CommentTexts { get; set; }
    }
}
