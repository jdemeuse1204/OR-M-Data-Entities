using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables
{
    [View("ContactOnly")]
    [Table("Contacts")]
    public class Contact : EntityStateTrackable
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public int ID { get; set; }

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
