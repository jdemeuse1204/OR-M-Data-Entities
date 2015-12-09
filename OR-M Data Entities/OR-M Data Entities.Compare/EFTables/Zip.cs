using System.ComponentModel.DataAnnotations.Schema;

namespace OR_M_Data_Entities.Compare.EFTables
{
    [Table("ZipCode")]
    public class Zip
    {
        public int ID { get; set; }

        public string Zip5 { get; set; }

        public string Zip4 { get; set; }

        // foreign key
        public int AddressID { get; set; }
        public Address Address { get; set; }
    }
}
