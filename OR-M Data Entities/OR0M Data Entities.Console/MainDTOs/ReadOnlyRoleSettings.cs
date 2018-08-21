using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("RoleSettings")]
    public class ReadOnlyRoleSettings
    {
        [Key]
        public int Id { get; set; }
        public bool CanViewManagerJobs { get; set; }
        public bool CanBeAssignedJobs { get; set; }
    }
}
