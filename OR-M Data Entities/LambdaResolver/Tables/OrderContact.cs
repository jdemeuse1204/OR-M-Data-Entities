﻿using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [LinkedServer("VINCADB", "OVM", "DBO")]
    public class OrderContact
    {
        [Key]
        public int ContactID { get; set; }

        public int OrderID { get; set; }

        [ForeignKey("ContactID")]
        public Contact Contact { get; set; }
    }
}
