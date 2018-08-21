using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("UserPasswordReset")]
    public class ReadOnlyUserPasswordReset
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset RequestedDateTime { get; set; }
        public DateTimeOffset ExpirationDateTime { get; set; }
        public string Token { get; set; }
    }
}
