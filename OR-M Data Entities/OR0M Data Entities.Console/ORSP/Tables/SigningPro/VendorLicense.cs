using System;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.SigningPro
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [Table("VendorLicenses")]
    public class VendorLicense
    {
        [Key]
        public int LicenseStateID { get; set; }

        public int VendorID { get; set; }

        public string StateCode { get; set; }

        public string LicenseNumber { get; set; }

        public DateTime LicenseExpirationDate { get; set; }

        public int LicenseTypeValue { get; set; }

        public bool Is5YearLicense { get; set; }

        public string ItemName { get; set; }

        public string Status { get; set; }
    }
}
