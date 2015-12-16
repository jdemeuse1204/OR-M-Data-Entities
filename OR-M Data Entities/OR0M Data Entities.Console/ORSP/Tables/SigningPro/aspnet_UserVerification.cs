using System;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace ORSigningPro.Common.Data.Tables.SigningPro
{
    public enum UserVerificationClassification
    {
        Authorize,
        PasswordReset
    }

    public class aspnet_UserVerification : EntityStateTrackable
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public DateTime ExpirationDate { get; set; }

        public int FailedAttempts { get; set; }

        public UserVerificationClassification Classification { get; set; }

        public string Data { get; set; }
    }
}
