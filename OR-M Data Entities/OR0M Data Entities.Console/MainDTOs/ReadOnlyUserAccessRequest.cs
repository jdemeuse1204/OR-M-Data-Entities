using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("UserAccessRequests")]
    public class ReadOnlyUserAccessRequest
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        [ForeignKey("UserAccessRequestId")]
        public List<ReadOnlyUserAccessRequestRole> RequestedRoles { get; set; }

        [ForeignKey("UserId")]
        public ReadOnlyUser User { get; set; }
    }
}
