using System;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    public class Address
    {
        public int ID { get; set; }

        public string Addy { get; set; }

        [ForeignKey(typeof(Appointment), "ID")]
        public Guid AppointmentID { get; set; }

        [AutoLoad]
        public Zip ZipCode { get; set; }
    }
}
