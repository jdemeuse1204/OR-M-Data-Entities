using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("Rules")]
    public class ReadOnlyRule
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string FunctionLogic { get; set; }
        public int Order { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}
