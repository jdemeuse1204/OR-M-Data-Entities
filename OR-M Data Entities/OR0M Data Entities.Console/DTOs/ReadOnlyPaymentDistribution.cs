using OR_M_Data_Entities.Lite.Mapping;
using System.Collections.Generic;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
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
