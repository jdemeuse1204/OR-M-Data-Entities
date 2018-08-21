using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main.User
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("Users")]
    public class ReadOnlyUserWIthUserInformation
    {
        [Key]
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int? UserInformationId { get; set; }

        [ForeignKey("UserInformationId")]
        public ReadOnlyUserInformation UserInformation { get; set; }
    }
}
