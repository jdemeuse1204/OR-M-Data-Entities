using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    [Table("Contacts")]
    public class Contact
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public int ID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [AutoLoad]
        public List<Appointment> Appointments { get; set; }
    }
}
