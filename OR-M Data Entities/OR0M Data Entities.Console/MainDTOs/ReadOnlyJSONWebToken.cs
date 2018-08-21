using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("JSONWebTokens")]
    public class ReadOnlyJSONWebToken
    {
        [Key]
        public Guid TokenId { get; set; }
        public Guid IssuedUserId { get; set; }
        public DateTimeOffset IssuedDateTime { get; set; }
        public bool IsBlackListed { get; set; }

        [ForeignKey("IssuedUserId")]
        public ReadOnlyUser User { get; set; }
    }
}
