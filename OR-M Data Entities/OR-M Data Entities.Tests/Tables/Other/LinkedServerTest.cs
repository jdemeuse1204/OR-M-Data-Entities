using System;
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
}
