using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
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
