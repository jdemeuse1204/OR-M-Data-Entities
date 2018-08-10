using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("PaymentDistributionSplits")]
    public class ReadOnlyPaymentDistributionSplit
    {
        [Key]
        public int Id { get; set; }
        public int PaymentDistributionId { get; set; }
        public Guid PayeeRoleId { get; set; }
        public decimal Percent { get; set; }
        public decimal Price { get; set; }
        public bool IsPercentageSplit { get; set; }

        [ForeignKey("PayeeRoleId")]
        public ReadOnlyRole PayeeRole { get; set; }
    }
}
