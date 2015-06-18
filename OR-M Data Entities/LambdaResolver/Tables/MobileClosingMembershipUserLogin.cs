using System;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    public class MobileClosingMembershipUserLogin
    {
        [Key]
        public int MobileClosingMembershipUserLoginID { get; set; }

        public Guid UserID { get; set; }

        public bool IsLockedOut { get; set; }

        public DateTimeOffset LastLockOutDate { get; set; }

        public int FailedPasswordAttemptCount { get; set; }

        public DateTimeOffset FailedPasswordAttemptWindowStart { get; set; }

        public long FailedPasswordAttemptWindowLength { get; set; }

        public bool IsActive { get; set; }

        [Unmapped]
        public TimeSpan FailedPasswordAttemptWindowSpan
        {
            get { return TimeSpan.FromTicks(FailedPasswordAttemptWindowLength); }
            set { FailedPasswordAttemptWindowLength = value.Ticks; }
        }
    }
}
