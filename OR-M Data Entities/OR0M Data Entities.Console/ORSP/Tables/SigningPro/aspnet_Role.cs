using System;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace ORSigningPro.Common.Data.Tables.SigningPro
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [Table("aspnet_Roles")]
    public class aspnet_Role : EntityStateTrackable
    {
        public Guid ApplicationId { get; set; }

        [Key]
        public Guid RoleId { get; set; }

        public string RoleName { get; set; }

        public string LoweredRoleName { get; set; }

        public string Description { get; set; }

        public bool TaskCanBeAssigned { get; set; }
    }
}
