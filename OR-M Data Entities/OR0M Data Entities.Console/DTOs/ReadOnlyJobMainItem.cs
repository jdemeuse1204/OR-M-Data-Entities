using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("JobMainItems")]
    public class ReadOnlyJobMainItem
    {
        [Key]
        public int JobId { get; set; }

        [Key]
        public Guid ItemId { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("ItemId")]
        public ReadOnlyItem Item { get; set; }
    }
}
