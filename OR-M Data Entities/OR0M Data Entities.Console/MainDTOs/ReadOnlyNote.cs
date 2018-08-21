using NickOfTime.ServiceModels.DataTransferObjects.ORM.Main.User;
using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("Notes")]
    public class ReadOnlyNote
    {
        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public DateTimeOffset? UpdatedDateTime { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public ReadOnlyUserWIthUserInformation CreatedByUser { get; set; }

        [ForeignKey("UpdatedByUserId")]
        public ReadOnlyUserWIthUserInformation UpdatedByUser { get; set; }
    }
}
