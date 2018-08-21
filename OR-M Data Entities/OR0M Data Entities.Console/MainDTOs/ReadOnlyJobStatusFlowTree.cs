using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("JobStatusFlowTree")]
    public class ReadOnlyJobStatusFlowTree
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int FlowId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int StatusId { get; set; }
    }
}
