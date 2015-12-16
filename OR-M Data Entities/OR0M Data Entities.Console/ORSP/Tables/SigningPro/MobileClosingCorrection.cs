using System;
using ORSigningPro.Common.Infrastructure.Enum;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace ORSigningPro.Common.Data.Tables.SigningPro
{
    public class MobileClosingCorrection : EntityStateTrackable
    {
        [Key]
        public int MobileClosingCorrectionID { get; set; }

        public MobileClosingCorrectionStatus StatusCode { get; set; }

        public string CorrectionText { get; set; }

        public DateTimeOffset? ConfirmedWithBorrowerDateTime { get; set; }

        public DateTimeOffset? DocsDownloadedPrintedDateTime { get; set; }

        public DateTimeOffset? CompletedDateTime { get; set; }

        public DateTimeOffset? DocumentsScannedDateTime { get; set; }

        public DateTimeOffset? DroppedDateTime { get; set; }

        public DateTimeOffset? DeletedDateTime { get; set; }

        public int MobileClosingID { get; set; }

        [Unmapped]
        public DateTimeOffset? AppointmentDateTime { get; set; }
    }
}
