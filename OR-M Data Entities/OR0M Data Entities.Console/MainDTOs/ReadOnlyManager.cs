using OR_M_Data_Entities.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("Managers")]
    public class ReadOnlyManager
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public Guid ManagerUserId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public ReadOnlyUser User { get; set; }
    }
}
