using System;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    [Table("UserAddresses")]
    public class UserAddress : EntityStateTrackable
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int AddressId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int UserId { get; set; }

        [ForeignKey("AddressId")]
        public Address Address { get; set; }
    }
}
