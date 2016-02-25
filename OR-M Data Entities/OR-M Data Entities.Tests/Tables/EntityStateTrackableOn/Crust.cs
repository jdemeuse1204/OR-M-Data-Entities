using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class Crust : EntityStateTrackable
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int? ToppingId { get; set; }

        [ForeignKey("ToppingId")]
        public Topping Topping { get; set; }
    }
}
