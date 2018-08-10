using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("AddressTypes")]
    public class ReadOnlyAddressType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
