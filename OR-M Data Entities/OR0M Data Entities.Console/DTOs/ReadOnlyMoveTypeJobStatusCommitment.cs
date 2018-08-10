using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("MoveTypeJobStatusCommitments")]
    public class ReadOnlyMoveTypeJobStatusCommitment
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MoveTypeId { get; set; }
        public int ExpectedStatusId { get; set; }
        public int CompletionHours { get; set; }
    }
}
