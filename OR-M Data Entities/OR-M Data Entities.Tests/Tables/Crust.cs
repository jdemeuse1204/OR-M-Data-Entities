using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    public class Crust
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int? ToppingId { get; set; }

        [ForeignKey("ToppingId")]
        public Topping Topping { get; set; }
    }
}
