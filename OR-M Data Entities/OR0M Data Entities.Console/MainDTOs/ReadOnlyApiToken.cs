using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("ApiTokens")]
    public class ReadOnlyApiToken
    {
        [Key]
        public Guid TokenId { get; set; }
        public string Token { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public bool IsActive { get; set; }
        public int SourceSystemId { get; set; }
    }
}
