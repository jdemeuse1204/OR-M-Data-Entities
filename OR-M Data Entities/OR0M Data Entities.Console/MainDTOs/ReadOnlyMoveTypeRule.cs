using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("MoveTypeRules")]
    public class ReadOnlyMoveTypeRule
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int RuleId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int MoveTypeId { get; set; }

        [ForeignKey("RuleId")]
        public ReadOnlyRule Rule { get; set; }

        [ForeignKey("MoveTypeId")]
        public ReadOnlyMoveType MoveType { get; set; }
    }
}
