using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Compare.EFTables
{
    [Table("Appointments")]
    public class Appointment
    {
        [Key]
        public Guid ID { get; set; }

        public int ContactID { get; set; }

        public Contact Contact { get; set; }

        public string Description { get; set; }

        public bool IsScheduled { get; set; }

        public virtual ICollection<Address> Address { get; set; }

    }
}
