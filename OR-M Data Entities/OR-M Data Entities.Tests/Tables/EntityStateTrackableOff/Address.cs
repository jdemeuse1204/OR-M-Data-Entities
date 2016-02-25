using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff
{
    public class Address 
    {
        public int ID { get; set; }

        public string Addy { get; set; }

        public Guid AppointmentID { get; set; }

        public int StateID { get; set; }

        [ForeignKey("StateID")]
        public StateCode State { get; set; }

        [ForeignKey("AddressID")]
        public List<Zip> ZipCode { get; set; }
    }
}
