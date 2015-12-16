using System;
using System.Collections.Generic;
using ORSigningPro.Common.Infrastructure.Enum;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace ORSigningPro.Common.Data.Tables.SigningPro
{
    [Table("aspnet_Users")]
    public class aspnet_User : EntityStateTrackable
    {
        [Key]
        [Column("UserId")]
        public Guid Id { get; set; }

        public Guid ApplicationId { get; set; }

        public string UserName { get; set; }

        public string LoweredUserName { get; set; }

        public string MobileAlias { get; set; }

        public bool IsAnonymous { get; set; }

        public DateTime LastActivityDate { get; set; }

        public int VendorID { get; set; }

        public UserType UserType { get; set; }

        public DateTimeOffset ActivationDate { get; set; }

        [ForeignKey("Id")]
        public aspnet_Membership aspnet_Membership { get; set; }
    }
}
