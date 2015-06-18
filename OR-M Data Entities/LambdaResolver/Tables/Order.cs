using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [Table("Orders")]
    [LinkedServer("VINCADB", "OVM", "DBO")]
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

        [ForeignKey("OrderID")]
        public OrderContact OrderContact { get; set; }

        [ForeignKey("OrderID")]
        public List<Order_Note> Order_Note { get; set; }
    }
}
