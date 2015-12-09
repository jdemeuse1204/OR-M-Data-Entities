using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.ORTSigningPro.Tables
{
    [LinkedServer("VINCADB", "OVM", "DBO")]
    public class Contact
    {
        [Key]
        public int ContactID { get; set; }

        public string Email { get; set; }

        public string HomePhone { get; set; }

        public string Mobile { get; set; }

        public int ContactRoleValue { get; set; }

    }
}
