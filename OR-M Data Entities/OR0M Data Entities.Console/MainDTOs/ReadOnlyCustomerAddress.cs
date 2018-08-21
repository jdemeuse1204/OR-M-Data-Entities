using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("CustomersAddresses")]
    public class ReadOnlyCustomerAddress
    {
        [DbGenerationOption(DbGenerationOption.None)]
        [Key]
        public Guid CustomerId { get; set; }

        [DbGenerationOption(DbGenerationOption.None)]
        [Key]
        public int AddressId { get; set; }

        [ForeignKey("AddressId")]
        public ReadOnlyAddress Address { get; set; }
    }
}
