using System;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Temp
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    public class SigningProSelectMobileClosing
    {
        public int MobileClosingID { get; set; }

        public string ReferenceId { get; set; }

        public string Borrowers { get; set; }

        public string CustomerName { get; set; }

        public DateTimeOffset? AppointmentDateTime { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string City { get; set; }

        public string CountyName { get; set; }

        public string StateCode { get; set; }

        public string ZipCode5 { get; set; }

        public string ZipCode4 { get; set; }

        public Guid? LockedByUserId { get; set; }

        [Unmapped]
        public bool IsLocked => LockedByUserId != null;

        public bool InProgress { get; set; }
    }
}
