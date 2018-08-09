using OR_M_Data_Entities.Lite;
using OR_M_Data_Entities.Lite.Mapping;
using System;
using System.Collections.Generic;
using System.Data;

namespace OR0M_Data_Entities.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var context = new DbSqlLiteContext("");

            var result = context.From<Contact>().FirstOrDefault(w => w.ContactID == 1);

        }

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

        public class User
        {
            [Key]
            public int ID { get; set; }

            public string Name { get; set; }
        }

        [Table("PhoneNumbers")]
        public class PhoneNumber
        {
            [Key]
            public int ID { get; set; }

            public string Phone { get; set; }

            public int? PhoneTypeID { get; set; }

            [ForeignKey("PhoneTypeID")]
            public PhoneType PhoneType { get; set; }
        }

        public class PhoneType
        {
            [Key]
            public int ID { get; set; }

            public string Type { get; set; }
        }

        [Table("Appointments")]
        public class Appointment 
        {
            [Key]
            public Guid ID { get; set; }

            public int ContactID { get; set; }

            public string Description { get; set; }

            public bool IsScheduled { get; set; }

            [ForeignKey("AppointmentID")]
            public List<Address> Address { get; set; }
        }

        public class Address 
        {
            [Key]
            public int ID { get; set; }

            public string Addy { get; set; }

            public Guid AppointmentID { get; set; }

            public int StateID { get; set; }

            [ForeignKey("StateID")]
            public StateCode State { get; set; }

            [ForeignKey("AddressID")]
            public List<Zip> ZipCode { get; set; }
        }

        public class StateCode
        {
            [Key]
            public int ID { get; set; }

            public string Value { get; set; }
        }

        [Table("ZipCode")]
        public class Zip
        {
            [Key]
            public int ID { get; set; }

            public string Zip5 { get; set; }

            public string Zip4 { get; set; }

            public int AddressID { get; set; }
        }

        public class Name
        {
            [Key]
            public int ID { get; set; }

            public string Value { get; set; }

            public int ContactID { get; set; }
        }
    }
}
