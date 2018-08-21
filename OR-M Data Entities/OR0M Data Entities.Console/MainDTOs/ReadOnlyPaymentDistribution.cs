using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("PaymentDistributions")]
    public class ReadOnlyPaymentDistribution
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [ForeignKey("PaymentDistributionId")]
        public List<ReadOnlyPaymentDistributionSplit> PaymentDistributionSplits { get; set; }
    }
}
