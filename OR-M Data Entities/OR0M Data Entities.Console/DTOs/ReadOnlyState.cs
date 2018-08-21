using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("States")]
    public class ReadOnlyState
    {
        [Key]
        public int StateId { get; set; }
        public string StateName { get; set; }
        public string StateAbbreviation { get; set; }
    }
}
