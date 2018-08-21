﻿using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("MoveTypes")]
    public class ReadOnlyMoveType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string PickUpName { get; set; }
        public int PickupDwellingTypeId { get; set; }
        public string DropOffName { get; set; }
        public int DropOffDwellingTypeId { get; set; }
        public int PricingTypeId { get; set; }
        public int MoveTypeAssignmentId { get; set; }
        public int PaymentDistributionId { get; set; }

        [ForeignKey("PaymentDistributionId")]
        public ReadOnlyPaymentDistribution PaymentDistribution { get; set; }
        [ForeignKey("PricingTypeId")]
        public ReadOnlyPricingType PricingType { get; set; }
        [ForeignKey("MoveTypeAssignmentId")]
        public ReadOnlyMoveTypeAssignment MoveTypeAssignment { get; set; }
        [ForeignKey("MoveTypeId")]
        public List<ReadOnlyMoveTypeJobStatusCommitment> JobStatusCommitments { get; set; }
    }
}
