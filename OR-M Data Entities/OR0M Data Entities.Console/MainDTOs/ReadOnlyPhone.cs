using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
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
