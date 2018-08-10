using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("CustomersAddresses")]
    public class ReadOnlyCustomerAddress
    {
        [Key]
        public Guid CustomerId { get; set; }

        [Key]
        public int AddressId { get; set; }

        [ForeignKey("AddressId")]
        public ReadOnlyAddress Address { get; set; }
    }
}
