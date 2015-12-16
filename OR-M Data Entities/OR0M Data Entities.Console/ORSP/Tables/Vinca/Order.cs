using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [Table("Orders")]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
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
        // ReSharper disable once InconsistentNaming
        public List<Order_Note> Order_Note { get; set; }
    }
}
