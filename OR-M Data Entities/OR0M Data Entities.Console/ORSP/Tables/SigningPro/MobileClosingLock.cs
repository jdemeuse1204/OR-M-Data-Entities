using System;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace ORSigningPro.Common.Data.Tables.SigningPro
{
    public class MobileClosingLock : EntityStateTrackable
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int MobileClosingID { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        // ReSharper disable once InconsistentNaming
        public Guid aspnetUserID { get; set; }

        public DateTimeOffset LockDate { get; set; }

        [ForeignKey("aspnetUserID")]
        public aspnet_User LockedByUser { get; set; }
    }
}
