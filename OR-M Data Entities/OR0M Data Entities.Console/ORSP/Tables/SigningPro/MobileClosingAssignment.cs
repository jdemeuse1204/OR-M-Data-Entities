using System;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace ORSigningPro.Common.Data.Tables.SigningPro
{
    public class MobileClosingAssignment : EntityStateTrackable
    {
        [Key]
        public int MobileClosingAssignmentID { get; set; }
        
        public int MobileClosingID { get; set; }

        public int VincaVendorID { get; set; }

        public DateTimeOffset WindowStartDate { get; set; }

        public DateTimeOffset WindowEndDate { get; set; }

        public DateTimeOffset? AcceptedDate { get; set; }

        public DateTimeOffset? DeclinedDate { get; set; }

        public decimal Price { get; set; }

        public decimal Distance { get; set; }
    }
}
