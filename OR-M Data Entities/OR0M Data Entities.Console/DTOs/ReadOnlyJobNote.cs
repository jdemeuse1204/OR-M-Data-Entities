using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("JobNotes")]
    public class ReadOnlyJobNote
    {
        [Key]
        public int JobId { get; set; }

        [Key]
        public int NoteId { get; set; }

        [ForeignKey("NoteId")]
        public ReadOnlyNote Note { get; set; }
    }
}
