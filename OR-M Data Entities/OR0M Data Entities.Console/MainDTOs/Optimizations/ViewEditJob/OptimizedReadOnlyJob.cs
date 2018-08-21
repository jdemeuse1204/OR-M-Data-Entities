using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main.Optimizations.ViewEditJob
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("Jobs")]
    public class OptimizedReadOnlyJob
    {
        [Key]
        public int JobId { get; set; }
        public int PlacementLocationId { get; set; }
        public string InternalNotes { get; set; }
        public int MoveTypeId { get; set; }
        public Guid CustomerId { get; set; }
        public decimal Mileage { get; set; }
        public int PaymentTypeId { get; set; }
        public int StatusId { get; set; }
        public int IntakeTypeId { get; set; }
        public int SourceSystemId { get; set; }
        public string TrackingNumber { get; set; }
        public Guid? AcceptedUserId { get; set; }
        public DateTimeOffset? WindowStartDateTime { get; set; }
        public DateTimeOffset? WindowEndDateTime { get; set; }
        public string ReferenceId { get; set; }
        public bool IsDifficult { get; set; }
        public int PaymentDistributionId { get; set; }

        [ForeignKey("PaymentDistributionId")]
        public ReadOnlyPaymentDistribution PaymentDistribution { get; set; }

        [ForeignKey("CustomerId")]
        public ReadOnlyCustomer Customer { get; set; }

        [ForeignKey("AcceptedUserId")]
        public OptimizedReadOnlyUser AcceptedUser { get; set; }

        [ForeignKey("IntakeTypeId")]
        public ReadOnlyJobIntakeType Source { get; set; }

        [ForeignKey("StatusId")]
        public ReadOnlyJobStatus Status { get; set; }

        [ForeignKey("PaymentTypeId")]
        public ReadOnlyPaymentType PaymentType { get; set; }

        [ForeignKey("MoveTypeId")]
        public OptimizedReadOnlyMoveType MoveType { get; set; }

        [ForeignKey("PlacementLocationId")]
        public ReadOnlyPlacementLocation PlacementLocation { get; set; }

        [ForeignKey("JobId")]
        public List<ReadOnlyJobAddress> JobAddresses { get; set; }

        [ForeignKey("JobId")]
        public List<ReadOnlyJobLineItem> JobLineItems { get; set; }

        [ForeignKey("JobId")]
        public List<ReadOnlyJobMainItem> JobMainItems { get; set; }

        [ForeignKey("JobId")]
        public List<ReadOnlyJobNote> JobNotes { get; set; }

        [ForeignKey("JobId")]
        public List<ReadOnlyJobAssignmentQueue> JobAssignmentQueue { get; set; }
    }
}
