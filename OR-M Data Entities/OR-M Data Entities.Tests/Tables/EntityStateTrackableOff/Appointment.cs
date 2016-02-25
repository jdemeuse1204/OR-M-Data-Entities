using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff
{
    [Table("Appointments")]
    public class Appointment 
    {   
        [DbGenerationOption(DbGenerationOption.Generate)]
        public Guid ID { get; set; }

        public int ContactID { get; set; }

        public string Description { get; set; }

        public bool IsScheduled { get; set; }

        [ForeignKey("AppointmentID")]
        public List<Address> Address { get; set; }
    }
}
