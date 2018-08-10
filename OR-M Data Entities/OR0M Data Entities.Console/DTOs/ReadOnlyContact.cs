using OR_M_Data_Entities.Lite.Mapping;
using System;
using System.Collections.Generic;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("Contacts")]
    public class ReadOnlyContact
    {
        [Key]
        public Guid ContactId { get; set; }
        public Guid CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleInitial { get; set; }

        [ForeignKey("ContactId")]
        public List<ReadOnlyContactPhoneNumber> PhoneNumbers { get; set; }

        [ForeignKey("ContactId")]
        public List<ReadOnlyContactEmailAddress> EmailAddresses { get; set; }

        [ForeignKey("ContactId")]
        public List<ReadOnlyContactsDesignations> Designations { get; set; }
    }
}
