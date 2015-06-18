using System;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    public class MobileClosingCorrection
    {
        [Key]
        public int MobileClosingCorrectionID { get; set; }

        public int VincaVendorID { get; set; }

        public string VincaOrderNumber { get; set; }

        public string VincaProductNumber { get; set; }

        public int SequenceNumber { get; set; }

        public int StatusCode { get; set; }

        public string CorrectionText { get; set; }

        public DateTimeOffset PresentedDateTime { get; set; }

        public DateTimeOffset? ConfirmedWithBorrowerDateTime { get; set; }

        public bool AppointmentRequired { get; set; }

        public DateTimeOffset? AppointmentDateTime { get; set; }

        public DateTimeOffset? DocsDownloadedPrintedDateTime { get; set; }

        public DateTimeOffset? CompletedDateTime { get; set; }

        public DateTimeOffset? DocumentsScannedDateTime { get; set; }

        public DateTimeOffset? VincaWorkAcceptedDateTime { get; set; }

        public DateTimeOffset? DroppedDateTime { get; set; }

        public DateTimeOffset? DeletedDateTime { get; set; }

        public int MobileClosingAddressID { get; set; }

        [ForeignKey("MobileClosingAddressID")]
        public MobileClosingAddress MobileClosingAddress { get; set; }

        [Unmapped]
        public object Details { get; set; }
    }
}
