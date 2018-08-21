using OR_M_Data_Entities.Lite.Mapping;
using System.Collections.Generic;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("MoveTypeAssignment")]
    public class ReadOnlyMoveTypeAssignment
    {
        [Key]
        public int Id { get; set; }
        public bool CanAssignMultipleUsers { get; set; }
        public bool ShouldBeAutoAccepted { get; set; }

        [ForeignKey("MoveTypeAssignmentId")]
        public List<ReadOnlyMoveTypeAssignmentRole> MoveTypeAssignmentRoles { get; set; }
    }
}
