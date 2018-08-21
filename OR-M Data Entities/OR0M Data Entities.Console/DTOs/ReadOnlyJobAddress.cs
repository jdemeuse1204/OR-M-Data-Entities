using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("JobsAddresses")]
    public class ReadOnlyJobAddress
    {
        [Key]
        public int JobId { get; set; }

        [Key]
        public int AddressId { get; set; }

        [ForeignKey("AddressId")]
        public ReadOnlyAddress Address { get; set; }
    }
}
