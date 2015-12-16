using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        public int OrderID { get; set; }
        public DateTime? ProductOrderDate { get; set; }

        public string ProductNumber { get; set; }

        public string VendorComments { get; set; }

        public string ReportToEmail { get; set; }

        public int ProductTypeValue { get; set; }

        public int ProductStatusValue { get; set; }

        public int CustomerProductID { get; set; }

        public string Note { get; set; }

        [Column("1stLoanAmount")]
        public decimal? FirstLoanAmount { get; set; }

        [Column("2ndLoanAmount")]
        public decimal? SecondLoanAmount { get; set; }

        public int? LoanCategory { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedUserID { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public Guid LastUpdatedUserID { get; set; }

        public byte[] LastChanged { get; set; }

        public int? SourceTypeID { get; set; }

        public string UniqueID { get; set; }

        public string FirstBorrowerName { get; set; }

        public string VendorCommentText { get; set; }

        [ForeignKey("OrderID")]
        public Order Order { get; set; }

        [ForeignKey("ProductID")]
        // ReSharper disable once InconsistentNaming
        public List<Ancillary_Note> Ancillary_Notes { get; set; }

        [ForeignKey("ProductID")]
        public AncillaryProduct AncillaryProduct { get; set; }

    }
}
