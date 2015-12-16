using System;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace ORSigningPro.Common.Data.Tables.SigningPro
{
    public class aspnet_Membership : EntityStateTrackable
    {
        public Guid ApplicationId { get; set; }

        [Key]
        public Guid UserId { get; set; }

        public string Password { get; set; }

        public int PasswordFormat { get; set; }

        public string PasswordSalt { get; set; }

        public string MobilePIN { get; set; }

        public string Email { get; set; }

        public string LoweredEmail { get; set; }

        public string PasswordQuestion { get; set; }

        public string PasswordAnswer { get; set; }

        [Column("IsApproved")]
        public bool IsNotAdminLockedOut { get; set; }

        public bool IsLockedOut { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime LastLoginDate { get; set; }

        public DateTime LastPasswordChangedDate { get; set; }

        public DateTime LastLockoutDate { get; set; }

        public int FailedPasswordAttemptCount { get; set; }

        public DateTime FailedPasswordAttemptWindowStart { get; set; }

        public int FailedPasswordAnswerAttemptCount { get; set; }

        public DateTime FailedPasswordAnswerAttemptWindowStart{ get; set; }

        public string Comment { get; set; }

        public bool IsRemoved { get; set; }
    }
}
