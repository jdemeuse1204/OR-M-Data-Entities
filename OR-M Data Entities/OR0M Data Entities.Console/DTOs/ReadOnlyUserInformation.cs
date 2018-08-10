using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("UserInformation")]
    public class ReadOnlyUserInformation
    {
        [Key]
        public int UserInformationId { get; set; }
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }
        public int? AddressId { get; set; }
    }
}
