using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("States")]
    public class ReadOnlyState
    {
        [Key]
        public int StateId { get; set; }
        public string StateName { get; set; }
        public string StateAbbreviation { get; set; }
    }
}
