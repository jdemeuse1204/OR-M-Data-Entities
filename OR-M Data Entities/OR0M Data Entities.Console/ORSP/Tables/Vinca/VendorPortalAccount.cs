using System;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    public class VendorPortalAccount 
    {
        [Key]
        [Column("VendorPortalAccountID")]
        public int Id { get; set; }

        public int VendorID { get; set; }

        [ForeignKey("VendorID")]
        public Vendor Vendor { get; set; }
        
        [Column("AccountName")]
        public string UserName { get; set; }

        public string Password { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedUserID { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public Guid LastUpdatedUserID { get; set; }

        public byte[] LastChanged { get; set; }

        public string Email { get; set; }
    }
}
