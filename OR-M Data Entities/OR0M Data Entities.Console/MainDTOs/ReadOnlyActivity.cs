using OR_M_Data_Entities.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("Activity")]
    public class ReadOnlyActivity
    {
        [Key]
        public int ActivityId { get; set; }
        public string ActivityHtml { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public bool IsResolved { get; set; }
    }
}
