using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
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
