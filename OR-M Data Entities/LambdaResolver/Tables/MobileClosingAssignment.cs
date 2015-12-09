using System;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    public class MobileClosingAssignment
    {
        [Key]
        public int MobileClosingAssignmentID { get; set; }
        
        public int MobileClosingID { get; set; }

        public int VincaVendorID { get; set; }

        public int SequenceNumber { get; set; }

        public DateTimeOffset WindowStartDate { get; set; }

        public DateTimeOffset WindowEndDate { get; set; }

        public DateTimeOffset? AcceptedDate { get; set; }

        public DateTimeOffset? DeclinedDate { get; set; }
    }
}
