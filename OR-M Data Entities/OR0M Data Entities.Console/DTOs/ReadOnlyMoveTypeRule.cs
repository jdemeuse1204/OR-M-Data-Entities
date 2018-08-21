using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("MoveTypeRules")]
    public class ReadOnlyMoveTypeRule
    {
        [Key]
        public int RuleId { get; set; }

        [Key]
        public int MoveTypeId { get; set; }

        [ForeignKey("RuleId")]
        public ReadOnlyRule Rule { get; set; }

        [ForeignKey("MoveTypeId")]
        public ReadOnlyMoveType MoveType { get; set; }
    }
}
