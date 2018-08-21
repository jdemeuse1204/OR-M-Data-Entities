using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("ContactsPhoneNumbers")]
    public class ReadOnlyContactPhoneNumber
    {
        [Key]
        public Guid ContactId { get; set; }

        [Key]
        public int PhoneNumberId { get; set; }

        [ForeignKey("PhoneNumberId")]
        public ReadOnlyPhone PhoneNumber { get; set; }
    }
}
