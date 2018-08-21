using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("Designations")]
    public class ReadOnlyDesignation
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
