using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OR_M_Data_Entities.Compare.EFTables
{
    [Table("Address")]
    public class Address
    {
        [Key]
        public int ID { get; set; }

        public string Addy { get; set; }

        // foreign key
        public Guid AppointmentID { get; set; }
        public Appointment Appointment { get; set; }

        // foreign key
        public int StateID { get; set; }
        public virtual StateCode State { get; set; }

        // foreign key
        public virtual ICollection<Zip> Zip { get; set; }
    }
}
