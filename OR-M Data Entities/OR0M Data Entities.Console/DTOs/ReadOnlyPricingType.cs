using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("PricingTypes")]
    public class ReadOnlyPricingType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
