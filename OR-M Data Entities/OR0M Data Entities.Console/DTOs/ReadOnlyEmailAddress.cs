using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("EmailAddresses")]
    public class ReadOnlyEmailAddress
    {
        [Key]
        public Guid EmailAddressId { get; set; }
        public string EmailAddress { get; set; }
    }
}
