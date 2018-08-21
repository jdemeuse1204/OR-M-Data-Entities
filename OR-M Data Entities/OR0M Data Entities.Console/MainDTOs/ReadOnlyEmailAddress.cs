using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("EmailAddresses")]
    public class ReadOnlyEmailAddress
    {
        [Key]
        public Guid EmailAddressId { get; set; }
        public string EmailAddress { get; set; }
    }
}
