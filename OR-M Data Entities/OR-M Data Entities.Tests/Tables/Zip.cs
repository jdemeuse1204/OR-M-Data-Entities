using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    [Table("ZipCode")]
    public class Zip
    {
        public int ID { get; set; }

        public string Zip5 { get; set; }

        public string Zip4 { get; set; }

        [ForeignKey(typeof(Address), "ID")]
        public int AddressID { get; set; }
    }
}
