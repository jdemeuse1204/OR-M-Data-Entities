using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("JobsAddresses")]
    public class ReadOnlyJobAddress
    {
        [DbGenerationOption(DbGenerationOption.None)]
        [Key]
        public int JobId { get; set; }

        [DbGenerationOption(DbGenerationOption.None)]
        [Key]
        public int AddressId { get; set; }

        [ForeignKey("AddressId")]
        public ReadOnlyAddress Address { get; set; }
    }
}
