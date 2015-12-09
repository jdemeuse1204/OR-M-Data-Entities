using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [View("ActiveOrder")]
    [Table("MobileClosing")]
    public class MobileClosing
    {
        [Key]
        public int MobileClosingID { get; set; }

        public int VincaVendorID { get; set; }

        public string VincaOrderNumber { get; set; }

        public string VincaProductNumber { get; set; }

        public int MobileClosingStatusCode { get; set; }

        public DateTimeOffset PresentedDateTime { get; set; }

        public DateTimeOffset? AcceptedDateTime { get; set; }

        public DateTimeOffset? DeclinedDateTime { get; set; }

        public DateTimeOffset? RescindedDateTime { get; set; }

        public DateTimeOffset? ConfirmedWithBorrowerDateTime { get; set; }

        public DateTimeOffset? DocsDownloadedPrintedDateTime { get; set; }

        public DateTimeOffset? InstructionsReviewedDateTime { get; set; }

        public DateTimeOffset? ArrivedAtSigningLocationDateTime { get; set; }

        public DateTimeOffset? CompletedDateTime { get; set; }

        public DateTimeOffset? DocumentsScannedDateTime { get; set; }

        public DateTimeOffset? VincaWorkAcceptedDateTime { get; set; }

        public DateTimeOffset? DroppedDateTime { get; set; }

        public DateTimeOffset? DeletedDateTime { get; set; }

        public DateTimeOffset? VincaPaymentSubmittedDateTime { get; set; }

        public decimal? VincaPaymentAmount { get; set; }

        public int MobileClosingAddressID { get; set; }

        public int MobileClosingPaymentCode { get; set; }

        public int VincaProductID { get; set; }

        [ForeignKey("MobileClosingAddressID")]
        public MobileClosingAddress MobileClosingAddress { get; set; }

        [ForeignKey("MobileClosingID")]
        public List<MobileClosingAssignment> MobileClosingAssignment { get; set; }

        [Unmapped]
        public object Details { get; set; }
    }
}
