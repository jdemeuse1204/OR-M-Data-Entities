using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [LinkedServer("VINCADB", "OVM", "DBO")]
    public class Order_Note
    {
        [Key]
        public int NoteID { get; set; }

        public int OrderID { get; set; }

        public int NoteTypeValue { get; set; }

        public string CommentTexts { get; set; }
    }
}
