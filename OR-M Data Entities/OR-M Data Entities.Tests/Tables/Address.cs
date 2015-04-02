using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    public class Address
    {
        public int ID { get; set; }

        public string Addy { get; set; }

        public Guid AppointmentID { get; set; }

        [ForeignKey("AddressID")]
        public List<Zip> ZipCode { get; set; }
    }
}
