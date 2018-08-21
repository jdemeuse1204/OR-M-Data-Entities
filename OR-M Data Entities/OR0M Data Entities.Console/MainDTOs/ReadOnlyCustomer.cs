using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
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
