using System;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [Table("aspnet_Roles")]
    [LinkedServer("VINCADB", "OVM", "DBO")]
    public class aspnet_Role
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
