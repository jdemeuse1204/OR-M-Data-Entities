using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("JobAssignmentQueue")]
    public class ReadOnlyJobAssignmentQueue
    {
        [Key]
        public int JobId { get; set; }

        [Key]
        public Guid UserId { get; set; }
        public bool IsDeclined { get; set; }
    }
}
