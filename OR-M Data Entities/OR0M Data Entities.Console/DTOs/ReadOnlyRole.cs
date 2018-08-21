using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("Roles")]
    public class ReadOnlyRole
    {
        [Key]
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public int RoleSettingsId { get; set; }
        [ForeignKey("RoleSettingsId")]
        public ReadOnlyRoleSettings Settings { get; set; }
    }
}
