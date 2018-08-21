using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("MoveTypeJobStatusCommitments")]
    public class ReadOnlyMoveTypeJobStatusCommitment
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MoveTypeId { get; set; }
        public int ExpectedStatusId { get; set; }
        public int CompletionHours { get; set; }
    }
}
