using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OR_M_Data_Entities.Compare.EFTables
{
    [Table("PhoneNumbers")]
    public class PhoneNumber 
    {
        [Key]
        public int ID { get; set; }

        public string Phone{ get; set; }

        public int PhoneTypeID { get; set; }

        [Required]
        public virtual PhoneType PhoneType { get; set; }

        public Contact Contact { get; set; }
    }
}
