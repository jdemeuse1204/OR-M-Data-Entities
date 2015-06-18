using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [LinkedServer("VINCADB", "OVM", "DBO")]
    public class Vendor
    {
        [Key]
        public int VendorID { get; set; }

        public string VendorCompanyName { get; set; }

        public int? CompanyID { get; set; }

        public int? SpecificCompanyID { get; set; }

        public int VendorStatusValue { get; set; }

        public int VendorTypeValue { get; set; }
       
        public string CellNumber { get; set; }
        
        public string PrimaryContactName { get; set; }
        
        public string OfficeNumber { get; set; }

        public string Email { get; set; }

        public string CompanyURL { get; set; }

        public string WebAccountName { get; set; }

        public string WebAccountPassword { get; set; }

        public string Notes { get; set; }

        public bool IsDeleted { get; set; }

        public decimal OtherRanking { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedUserID { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public Guid LastUpdatedUserID { get; set; }

        public byte[] LastChanged { get; set; }

        public bool IsSubVendor { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        [ForeignKey("VendorID")]
        public List<VendorFeeByProduct> VendorFeeByProducts { get; set; }
        
        [ForeignKey("VendorID")]
        public virtual List<VendorAddress> VendorAddresses { get; set; }

        [ForeignKey("VendorID")]
        public List<VendorRanking> VendorRankings { get; set; }

        [ForeignKey("VendorStatusValue")]
        public VendorStatus VendorStatus { get; set; }

        [Unmapped]
        public string StatusAbbreviation { get; set; }
    }
}
