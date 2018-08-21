using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("MoveTypeAssignmentRoles")]
    public class ReadOnlyMoveTypeAssignmentRole
    {
        [Key]
        public int MoveTypeAssignmentId { get; set; }

        [Key]
        public Guid RoleId { get; set; }

        [ForeignKey("RoleId")]
        public ReadOnlyRole Role { get; set; }
    }
}
