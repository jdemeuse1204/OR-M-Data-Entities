using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
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
