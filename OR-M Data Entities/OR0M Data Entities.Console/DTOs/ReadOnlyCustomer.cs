using OR_M_Data_Entities.Lite.Mapping;
using System;
using System.Collections.Generic;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("Customers")]
    public class ReadOnlyCustomer
    {
        [Key]
        public Guid CustomerId { get; set; }
        public int ClientTypeId { get; set; }
        public string Notes { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }

        [ForeignKey("CustomerId")]
        public List<ReadOnlyContact> Contacts { get; set; }

        [ForeignKey("CustomerId")]
        public List<ReadOnlyCustomerAddress> CustomerAddresses { get; set; }
    }
}
