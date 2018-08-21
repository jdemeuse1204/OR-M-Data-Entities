using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("PaymentTypes")]
    public class ReadOnlyPaymentType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
