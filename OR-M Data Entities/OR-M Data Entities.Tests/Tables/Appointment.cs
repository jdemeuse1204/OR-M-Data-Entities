using System;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    [Table("Appointments")]
    public class Appointment
    {   
        [DbGenerationOption(DbGenerationOption.Generate)]
        public Guid ID { get; set; }

        public int ContactID { get; set; }

        public string Description { get; set; }
    }
}
