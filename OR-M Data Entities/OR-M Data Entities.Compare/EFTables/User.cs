using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Compare.EFTables
{
    [Table("User")]
    public class User
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }

        [ForeignKey("ID")]
        public Contact Contact { get; set; }
    }
}
