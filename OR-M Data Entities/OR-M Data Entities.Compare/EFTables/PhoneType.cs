using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OR_M_Data_Entities.Compare.EFTables
{
    [Table("PhoneType")]
    public class PhoneType 
    {
        [Key]
        public int ID { get; set; }

        public string Type { get; set; }

        public PhoneNumber PhoneNumbers { get; set; }
    }
}
