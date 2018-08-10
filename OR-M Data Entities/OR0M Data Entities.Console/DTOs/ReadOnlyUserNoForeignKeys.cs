using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("Users")]
    public class ReadOnlyUserNoForeignKeys
    {
        [Key]
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset LastActivityDate { get; set; }
        public int? UserInformationId { get; set; }
        public bool HasTemporaryPassword { get; set; }
        public int UserMembershipId { get; set; }
    }
}
