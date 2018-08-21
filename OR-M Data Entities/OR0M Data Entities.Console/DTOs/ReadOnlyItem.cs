using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("Items")]
    public class ReadOnlyItem
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Modifier { get; set; }
        public string SKU { get; set; }
        public string UPC { get; set; }
        public int ItemTypeId { get; set; }
        public decimal? Weight { get; set; }
        public int EstimatedMinutesToComplete { get; set; }
        public int SourceId { get; set; }
    }
}
