using System;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables
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

        [ForeignKey("Id")]
        public aspnet_Membership aspnet_Memberships { get; set; }
    }
}
