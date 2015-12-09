using System.ComponentModel.DataAnnotations.Schema;

namespace OR_M_Data_Entities.Compare.EFTables
{
    [Table("Name")]
    public class Name
    {
        public int ID { get; set; }

        public string Value { get; set; }

        public int ContactID { get; set; }

        public Contact Contact { get; set; }
    }
}
