using System;
using System.Data;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    // ReSharper disable once InconsistentNaming
    public class AncillaryProduct_Vendor : EntityStateTrackable
    {
        [Key]
        public int VendorOrderID { get; set; }

        public string VendorOrderNumber { get; set; }

        public int ProductID { get; set; }

        public int VendorID { get; set; }

        public int VendorProductID { get; set; }

        public int VendorOrderStatusValue { get; set; }

        public decimal? AdjustmentFee { get; set; }

        public decimal? ActualFee { get; set; }

        public decimal? VendorFee { get; set; }

        public DateTime? SentDate { get; set; }

        public bool IsConfirmed { get; set; }

        public DateTime? ConfirmedDate { get; set; }

        public bool IsReceived { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public bool IsWorkRejected { get; set; }

        public DateTime? WorkRejectedDate { get; set; }

        public int? ReceivedTypeValue { get; set; }

        public int? TurnAroundTime { get; set; }

        public bool IsWorkAccepted { get; set; }

        public DateTime? WorkAcceptedDate { get; set; }

        public bool IsPaid { get; set; }

        public DateTime? PaidDate { get; set; }

        public DateTime? ProjectedDueDate { get; set; }

        public string Comments { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedUserId { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public Guid LastUpdatedUserId { get; set; }

        [DbType(SqlDbType.Timestamp)]
        public byte[] LastChanged { get; set; }

        public string CommentText { get; set; }

        public int Position { get; set; }

        public DateTime? FollowUpDate { get; set; }

        public bool IsVendorAutoAssigned { get; set; }
    }
}
