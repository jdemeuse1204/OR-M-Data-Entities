using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [Table("MobileClosingUserRoles")]
    public class MobileClosingUserRole
    {
        [Key]
        public int MobileClosingUserRoleID { get; set; }

        public string Name { get; set; }
    }
}
