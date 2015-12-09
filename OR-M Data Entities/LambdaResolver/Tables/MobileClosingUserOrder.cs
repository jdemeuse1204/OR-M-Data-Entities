using System;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [Table("MobileClosingUserOrder")]
    public class MobileClosingUserOrder
    {
        [Key]
        public int MobileClosingUserOrderID { get; set; }

        public int VendorPortalAccountID { get; set; }

        public int MobileClosingID { get; set; }

        public virtual MobileClosing MobileClosing { get; set; }

        public int VincaVendorID { get; set; }

        public string VincaOrderNumber { get; set; }

        public string VincaProductNumber { get; set; }

        public bool IsVisible { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

    }
}
