using OR_M_Data_Entities.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("UserMemberships")]
    public class ReadOnlyUserMembership
    {
        [Key]
        public int UserMembershipId { get; set; }

        public string Password { get; set; }

        public string PasswordSalt { get; set; }

        public string EmailAddress { get; set; }

        public int PasswordQuestionId { get; set; }

        public string PasswordAnswer { get; set; }

        public bool IsApproved { get; set; }

        public bool IsLockedOut { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset? LastLoginDate { get; set; }

        public DateTimeOffset? LastPasswordChangedDate { get; set; }

        public int FailedPasswordAttemptCount { get; set; }

        public DateTimeOffset? FailedPasswordAttemptWindowStart { get; set; }

        public int FailedPasswordAnswerAttemptCount { get; set; }

        public DateTimeOffset? FailedPasswordAnswerAttemptWindowStart { get; set; }
    }
}
