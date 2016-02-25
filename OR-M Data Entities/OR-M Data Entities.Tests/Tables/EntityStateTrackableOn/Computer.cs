using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class Computer : EntityStateTrackable
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ProcessorId { get; set; }

        [ForeignKey("ProcessorId")]
        public Processor Processor { get; set; }

        public bool IsCustom { get; set; }

        [ForeignKey("ComputerId")]
        public List<History> History { get; set; } 
    }
}
