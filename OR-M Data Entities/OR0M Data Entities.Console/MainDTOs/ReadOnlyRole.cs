using OR_M_Data_Entities.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
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
