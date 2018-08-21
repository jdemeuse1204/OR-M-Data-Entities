using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{

    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("UserAccessPreApprovals")]
    public class ReadOnlyUserAccessPreApproval
    {
        [Key]
        public int Id { get; set; }
        public int? PhoneNumberId { get; set; }
        public Guid? EmailAddressId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public Guid RoleId { get; set; }

        [ForeignKey("RoleId")]
        public ReadOnlyRole Role { get; set; }

        [ForeignKey("PhoneNumberId")]
        public ReadOnlyPhone Phone { get; set; }

        [ForeignKey("EmailAddressId")]
        public ReadOnlyEmailAddress Email { get; set; }
    }
}
