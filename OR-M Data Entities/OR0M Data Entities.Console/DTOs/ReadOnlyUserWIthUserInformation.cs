using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.User
{
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
