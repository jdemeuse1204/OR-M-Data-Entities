using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("UsersRoles")]
    public class ReadOnlyUsersRoles
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public Guid UserId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public Guid RoleId { get; set; }

        [ForeignKey("RoleId")]
        public ReadOnlyRole Role { get; set; }
    }
}
