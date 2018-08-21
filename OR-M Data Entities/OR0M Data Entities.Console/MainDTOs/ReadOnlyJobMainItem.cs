using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{ 
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("JobMainItems")]
    public class ReadOnlyJobMainItem
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int JobId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public Guid ItemId { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("ItemId")]
        public ReadOnlyItem Item { get; set; }
    }
}
