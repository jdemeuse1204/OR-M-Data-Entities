using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("JobAssignmentQueue")]
    public class ReadOnlyJobAssignmentQueue
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int JobId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public Guid UserId { get; set; }
        public bool IsDeclined { get; set; }
    }
}
