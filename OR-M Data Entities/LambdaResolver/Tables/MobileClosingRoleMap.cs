using System;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [Table("MobileClosingRoleMappings")]
    public class MobileClosingRoleMap
    { 
        [Key]
        public int MoblieClosingRoleID { get; set; }

        public string MobileClosingRoleName { get; set; }

        [Key]
        public Guid VincaRoleID { get; set; }

        public string VincaRoleName { get; set; }
    }
}
