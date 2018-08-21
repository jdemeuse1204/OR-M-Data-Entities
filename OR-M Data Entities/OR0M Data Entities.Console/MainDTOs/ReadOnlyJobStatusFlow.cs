using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("JobStatusFlows")]
    public class ReadOnlyJobStatusFlow
    {
        [Key]
        public int Id { get; set; }
        public int MoveTypeId { get; set; }
        public int StatusId { get; set; }
        [ForeignKey("FlowId")]
        public List<ReadOnlyJobStatusFlowTree> JobStatusFlowTrees { get; set; }
    }
}
