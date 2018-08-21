using OR_M_Data_Entities.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("DwellingTypes")]
    public class ReadOnlyDwellingType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
