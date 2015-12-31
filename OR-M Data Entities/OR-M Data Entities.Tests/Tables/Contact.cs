using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Scripts.Base;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables
{
    [View("ContactOnly", "ContactAndPhone")]
    [Table("Contacts")]
    public class Contact : EntityStateTrackable, IReadScript<Contact>
    {
        [Key]
        [Column("ID")]
        [DbGenerationOption(DbGenerationOption.Generate)]
        public int ContactID { get; set; }

        [DbGenerationOption(DbGenerationOption.Generate)]
        public int Test { get; set; }

        [DbGenerationOption(DbGenerationOption.Generate)]
        public Guid TestUnique { get; set; }

        [MaxLength(25)]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int? PhoneID { get; set; }

        public int CreatedByUserID { get; set; }

        public int EditedByUserID { get; set; }

        [ForeignKey("CreatedByUserID")]
        public User CreatedBy { get; set; }

        [ForeignKey("EditedByUserID")]
        public User EditedBy { get; set; }

        [ForeignKey("PhoneID")]
        public PhoneNumber Number { get; set; }

        [ForeignKey("ContactID")]
        public List<Appointment> Appointments { get; set; }

        [ForeignKey("ContactID")]
        public List<Name> Names { get; set; }
    }
}
