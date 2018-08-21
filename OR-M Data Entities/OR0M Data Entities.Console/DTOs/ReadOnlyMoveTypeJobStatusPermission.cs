using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("MoveTypeJobStatusPermissions")]
    public class ReadOnlyMoveTypeJobStatusPermission
    {
        [Key]
        public int Id { get; set; }
        public int MoveTypeId { get; set; }
        public int JobStatusId { get; set; }
        public Guid RoleId { get; set; }
    }
}
