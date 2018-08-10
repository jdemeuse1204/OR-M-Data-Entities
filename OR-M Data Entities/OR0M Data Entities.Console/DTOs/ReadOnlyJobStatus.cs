using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("JobStatuses")]
    public class ReadOnlyJobStatus
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int SequenceNumber { get; set; }
        public bool IsDeleted { get; set; }
    }
}
