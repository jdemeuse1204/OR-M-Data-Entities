using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("PhoneNumbers")]
    public class ReadOnlyPhone
    {
        [Key]
        public int PhoneNumberId { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberExtension { get; set; }
        public int PhoneNumberTypeId { get; set; }
    }
}
