using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("UserRegistrations")]
    public class ReadOnlyUserRegistration
    {
        [Key]
        public Guid RegistrationId { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset ExpirationDateTime { get; set; }
        public DateTimeOffset SendDateTime { get; set; }
    }
}
