using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OR_M_Data_Entities.Compare.EFTables
{
    [Table("StateCode")]
    public class StateCode 
    {
        public int ID { get; set; }

        public string Value { get; set; }

        public ICollection<Address> Addresses { get; set; } 
    }
}
