using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("ContactsEmailAddresses")]
    public class ReadOnlyContactEmailAddress
    {
        [Key]
        public Guid ContactId { get; set; }

        [Key]
        public Guid EmailAddressId { get; set; }

        [ForeignKey("EmailAddressId")]
        public ReadOnlyEmailAddress EmailAddress { get; set; }
    }
}
