using NickOfTime.ServiceModels.DataTransferObjects.ORM.User;
using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
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
