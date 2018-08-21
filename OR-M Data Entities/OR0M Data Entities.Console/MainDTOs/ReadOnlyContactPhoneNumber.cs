using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("ContactsPhoneNumbers")]
    public class ReadOnlyContactPhoneNumber
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public Guid ContactId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int PhoneNumberId { get; set; }

        [ForeignKey("PhoneNumberId")]
        public ReadOnlyPhone PhoneNumber { get; set; }
    }
}
