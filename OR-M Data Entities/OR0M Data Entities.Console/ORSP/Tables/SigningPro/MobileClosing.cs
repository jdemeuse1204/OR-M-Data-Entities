using System;
using System.Collections.Generic;
using ORSigningPro.Common.BusinessObjects.Order.Base;
using ORSigningPro.Common.Data.Tables.Vinca;
using ORSigningPro.Common.Infrastructure.Enum;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;
using System.Linq;

namespace ORSigningPro.Common.Data.Tables.SigningPro
{
    public class MobileClosing : EntityStateTrackable
    {
        public MobileClosing()
        {
            Details = new OrderDetails();
        }

        [Key]
        public int MobileClosingID { get; set; }

        public int VincaVendorID { get; set; }

        public string VincaOrderNumber { get; set; }

        public string VincaProductNumber { get; set; }

        public MobileClosingStatus MobileClosingStatusCode { get; set; }

        public DateTimeOffset? ConfirmedWithBorrowerDateTime { get; set; }

        public DateTimeOffset? DocsDownloadedPrintedDateTime { get; set; }

        public DateTimeOffset? InstructionsReviewedDateTime { get; set; }

        public DateTimeOffset? ArrivedAtSigningLocationDateTime { get; set; }

        public DateTimeOffset? CompletedDateTime { get; set; }

        public DateTimeOffset? DocumentsScannedDateTime { get; set; }

        public DateTimeOffset? DroppedDateTime { get; set; }

        public DateTimeOffset? DeletedDateTime { get; set; }

        public decimal? VincaPaymentAmount { get; set; }

        public MobileClosingPaymentType MobileClosingPaymentCode { get; set; }

        public decimal Price { get; set; }

        public string Borrowers { get; set; }

        public string CustomerName { get; set; }

        public bool DeclinedEmailSent { get; set; }

        public int? VincaVendorProductID { get; set; }

        public int VincaProductID { get; set; }

        [PseudoKey("VincaProductID", "ProductID")] // VendorOrderStatus should be a 3 = confirmed
        public List<AncillaryProduct_Vendor> AncillaryProductVendors { get; set; }

        [PseudoKey("VincaProductID", "ProductID")]
        public AncillaryProduct AncillaryProduct { get; set; }

        [ForeignKey("MobileClosingID")]
        public List<MobileClosingAddress> MobileClosingAddress { get; set; }

        [ForeignKey("MobileClosingID")]
        public List<MobileClosingAssignment> MobileClosingAssignments { get; set; }

        [ForeignKey("MobileClosingID")]
        public List<MobileClosingCorrection> MobileClosingCorrections { get; set; }

        [ForeignKey("MobileClosingID")]
        public List<MobileClosingLock> MobileClosingLock { get; set; }

        [Unmapped]
        public DateTimeOffset? AppointmentDateTime => AncillaryProduct?.ActualAppointmentDate;

        [Unmapped]
        public DateTimeOffset? CorrectionAppointmentDateTime
            =>
                AncillaryProductVendors?.FirstOrDefault(w => !(new[] { 2, 8 }).Contains(w.VendorOrderStatusValue))?
                    .FollowUpDate;

        [Unmapped]
        public OrderDetails Details { get; set; }

        [Unmapped]
        public string ReferenceId
            => string.IsNullOrWhiteSpace(VincaOrderNumber) || string.IsNullOrWhiteSpace(VincaProductNumber)
                ? string.Empty
                : $"{VincaOrderNumber}-{VincaProductNumber}";

        [Unmapped]
        public bool InProgress => MobileClosingAssignments != null;

        [Unmapped]
        public bool IsLocked => MobileClosingLock != null;

        [Unmapped]
        public bool IsTiedToVendorInVinca
        {
            get
            {
                if (AncillaryProductVendors == null) return false;

                var ancillaryProductVendor = AncillaryProductVendors.OrderByDescending(w => w.CreatedDate)
                    .FirstOrDefault(w => w.VendorID == VincaVendorID);

                return ancillaryProductVendor != null && ancillaryProductVendor.VendorID == VincaVendorID;
            }
        }

        [Unmapped]
        public bool IsVendorCancelledInVinca
        {
            get
            {
                if (!IsTiedToVendorInVinca) return false;

                return AncillaryProductVendors?.OrderByDescending(w => w.CreatedDate)
                        .First(w => w.VendorID == VincaVendorID)
                        .VendorOrderStatusValue == 2;

            }
        }
    }
}
