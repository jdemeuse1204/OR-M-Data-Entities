using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("Rules")]
    public class ReadOnlyRule
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string FunctionLogic { get; set; }
        public int Order { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}
