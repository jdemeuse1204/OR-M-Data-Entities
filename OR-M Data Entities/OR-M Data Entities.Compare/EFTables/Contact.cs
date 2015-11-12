using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OR_M_Data_Entities.Compare.EFTables
{
    [Table("Contacts")]
    public class Contact
    {
        [Key]
        [Column("ID")]
        public int ContactID { get; set; }

        public int Test { get; set; }

        public Guid TestUnique { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }

        public int CreatedByUserID { get; set; }

        [ForeignKey("CreatedByUserID")]
        public virtual User CreatedByUser { get; set; }

        public int EditedByUserID { get; set; }

        [ForeignKey("EditedByUserID")]
        public virtual User EditedByUser { get; set; }

        public int? PhoneID { get; set; }
       
        [Required]
        public virtual PhoneNumber PhoneNumbers { get; set; }
    }
}
