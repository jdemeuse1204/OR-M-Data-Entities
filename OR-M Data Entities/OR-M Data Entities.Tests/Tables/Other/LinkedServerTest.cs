using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables.Other
{
    [Table("Orders")]
    [LinkedServer("SERVER", "DATABASE", "SCHEMA")]
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        public string CustomerName { get; set; }

        public string OrderNumber { get; set; }

        public string FirstBorrowerName { get; set; }

        public string CustomerLoanNumber { get; set; }

        public string StateCode { get; set; }

        public DateTime CreatedDate { get; set; }

        [ForeignKey("OrderID")]
        public OrderAddress OrderAddress { get; set; }
    }

    [LinkedServer("SERVER", "DATABASE", "SCHEMA")]
    public class OrderAddress
    {
        [Key]
        public int OrderID { get; set; }


        public int AddressID { get; set; }
    }

    public class Test
    {
        [Key]
        public int MobileClosingID { get; set; }

        public int TestVendorID { get; set; }

        public string TestOrderNumber { get; set; }

        public string TestProductNumber { get; set; }

        public DateTimeOffset? ConfirmedWithBorrowerDateTime { get; set; }

        public DateTimeOffset? DocsDownloadedPrintedDateTime { get; set; }

        public DateTimeOffset? InstructionsReviewedDateTime { get; set; }

        public DateTimeOffset? ArrivedAtSigningLocationDateTime { get; set; }

        public DateTimeOffset? CompletedDateTime { get; set; }

        public DateTimeOffset? DocumentsScannedDateTime { get; set; }

        public DateTimeOffset? DroppedDateTime { get; set; }

        public DateTimeOffset? DeletedDateTime { get; set; }

        public decimal? TestPaymentAmount { get; set; }

        public decimal Price { get; set; }

        public string Borrowers { get; set; }

        public string CustomerName { get; set; }

        public bool DeclinedEmailSent { get; set; }

        public int? TestVendorProductID { get; set; }

        public int TestProductID { get; set; }

        [ForeignKey("TestProductID")]
        public Product Product { get; set; }

        [PseudoKey("TestProductID", "ProductID")] // VendorOrderStatus should be a 3 = confirmed
        public List<Test_Test> Tests { get; set; }
    }

    [LinkedServer("SERVER", "DATABASE", "SCHEMA")]
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
    }

    [LinkedServer("SERVER", "DATABASE", "SCHEMA")]
    // ReSharper disable once InconsistentNaming
    public class Test_Test 
    {
        [Key]
        public int VendorOrderID { get; set; }

        public string VendorOrderNumber { get; set; }

        public int ProductID { get; set; }

        public int VendorID { get; set; }

        public int VendorProductID { get; set; }

        public int VendorOrderStatusValue { get; set; }

        public decimal? AdjustmentFee { get; set; }

        public decimal? ActualFee { get; set; }

        public decimal? VendorFee { get; set; }

        public DateTime? SentDate { get; set; }

        public bool IsConfirmed { get; set; }

        public DateTime? ConfirmedDate { get; set; }

        public bool IsReceived { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public bool IsWorkRejected { get; set; }

        public DateTime? WorkRejectedDate { get; set; }

        public int? ReceivedTypeValue { get; set; }

        public int? TurnAroundTime { get; set; }

        public bool IsWorkAccepted { get; set; }

        public DateTime? WorkAcceptedDate { get; set; }

        public bool IsPaid { get; set; }

        public DateTime? PaidDate { get; set; }

        public DateTime? ProjectedDueDate { get; set; }

        public string Comments { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedUserId { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public Guid LastUpdatedUserId { get; set; }

        public byte[] LastChanged { get; set; }

        public string CommentText { get; set; }

        public int Position { get; set; }

        public DateTime? FollowUpDate { get; set; }

        public bool IsVendorAutoAssigned { get; set; }
    }
}
