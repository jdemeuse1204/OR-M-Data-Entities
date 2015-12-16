﻿using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    public class VendorStatus
    {
        [Key]
        public int ItemValue { get; set; }

        public string ItemName { get; set; }

        public string Abbreviation { get; set; }
    }
}
