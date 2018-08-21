using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("Users")]
    public class ReadOnlyUser
    {
        [Key]
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset LastActivityDate { get; set; }
        public int? UserInformationId { get; set; }
        public bool HasTemporaryPassword { get; set; }
        public int UserMembershipId { get; set; }

        [ForeignKey("UserMembershipId")]
        public ReadOnlyUserMembership UserMembership { get; set; }
        [ForeignKey("UserInformationId")]
        public ReadOnlyUserInformation UserInformation { get; set; }
        [ForeignKey("UserId")]
        public List<ReadOnlyUsersRoles> UsersRoles { get; set; }
    }
}
