using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main.Optimizations.ViewEditJob
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("Users")]
    public class OptimizedReadOnlyUser
    {
        [Key]
        public Guid UserId { get; set; }
        public string UserName { get; set; }
    }
}
