using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Scripts;
using OR_M_Data_Entities.Scripts.Base;
using OR_M_Data_Entities.Tracking;

namespace OR0M_Data_Entities.Console
{
    public class SqlContext : DbSqlContext
    {
        public SqlContext()
            : base("sqlExpress")
        {
            Configuration.IsLazyLoading = true;

            Configuration.UseTransactions = true;
            Configuration.ConcurrencyChecking.IsOn = true;
            Configuration.ConcurrencyChecking.ViolationRule = ConcurrencyViolationRule.OverwriteAndContinue;

            OnConcurrencyViolation += OnOnConcurrencyViolation;

            OnSqlGeneration += OnOnSqlGeneration;
        }

        private void OnOnSqlGeneration(string sql)
        {
            return;
            using (var writetext = File.AppendText("C:\\users\\jdemeuse\\desktop\\OR-M Sql.txt"))
            {
                writetext.WriteLine(sql);
                writetext.WriteLine("\r\r");
            }
        }

        private void OnOnConcurrencyViolation(object entity)
        {

        }
    }

    public class Test
    {
        public int Id { get; set; }

        public string Phone { get; set; }

        public Test2 Item { get; set; }
    }

    public class Test2
    {
        public int TestingId { get; set; }

        public string FirstName { get; set; }
    }

    internal class Program
    {
        private static bool Test(int i)
        {
            return false;
        }



        private static void Main(string[] args)
        {
            var context = new SqlContext();

            //var sedfsdf = context.From<Zip>()
            //    .InnerJoin(context.From<StateCode>(),
            //        contact => contact.ID,
            //        appointment => appointment.ID,
            //        (contact, appointment) => appointment).FirstOrDefault();
            //context.From<Contact>()
            //    .Where(w => w.ContactID == 1)
            //    .Select(w => new Test
            //    {
            //        Id = w.CreatedByUserID,
            //        Phone = w.Number.Phone,
            //        Item = new Test2
            //        {
            //            FirstName = w.FirstName,
            //            TestingId = w.ContactID
            //        }
            //    });

            //    .Where(w => w.ContactID == w.Appointments.First(q => q.ID == Guid.Empty).ContactID);
            //var sdfsdf = context.From<Contact>().IncludeTo("Appointments").FirstOrDefault(w => w.ContactID == 1);

           // sdfsdf.FirstName = "James";

            //context.SaveChanges(sdfsdf);

            var s = DateTime.Now;
            var t = context.Find<Contact>(1);
            var e = DateTime.Now;

            System.Console.WriteLine((e - s).Milliseconds);

            t.FirstName = "Different";

            context.SaveChanges(t);

            var sdsdfgf = context.From<Contact>().OrderByDescending(w => w.ContactID).Select(w => w.ContactID);

            var test = context.From<Contact>().Count(w => w.ContactID == 1);
            var sdgf = context.From<Contact>().OrderByDescending(w => w.ContactID).FirstOrDefault();
            var sdgdf = context.From<Contact>().Any(w => w.ContactID == 2);


            System.Console.WriteLine(sdgf);

            System.Console.WriteLine((e - s).Milliseconds);

            var c1 = context.Find<Contact>(1);

            if (sdgf != null)
            {
                sdgf.Appointments = new List<Appointment>();
            }

            //c1.FirstName = "WINing!";

            //context.SaveChanges(c1);

            //var xy = new Contact
            //{
            //    FirstName = "James"
            //};

            //context.SaveChanges(xy);

            var x = new Contact
            {
                CreatedBy = new User
                {
                    Name = "James Demeuse"
                },
                EditedBy = new User
                {
                    Name = "Different User"
                },
                FirstName = "Test",
                LastName = "User",
                Names = new List<Name>
                {
                    new Name
                    {
                        Value = "Win!"
                    },
                    new Name
                    {
                        Value = "FTW!"
                    }
                },
                Number = new PhoneNumber
                {
                    Phone = "555-555-5555",
                    PhoneType = new PhoneType
                    {
                        Type = "Cell"
                    }
                },
                Appointments = new List<Appointment>
                {
                    new Appointment
                    {
                        Description = "Appointment 1",
                        IsScheduled = false,
                        Address = new List<Address>
                        {
                            new Address
                            {
                                Addy = "1234 First Ave South",
                                State = new StateCode
                                {
                                    Value = "MN"
                                },
                                ZipCode = new List<Zip>
                                {
                                    new Zip
                                    {
                                        Zip4 = "5412",
                                        Zip5 = "55555"
                                    },
                                    new Zip
                                    {
                                        Zip5 = "12345"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            context.SaveChanges(x);

            //var x = context.Find<Contact>(2077);

            //x.FirstName = "WINss";

            //context.Delete(x);

            //var test = context.GetHealth<Contact>(DatabaseStoreType.SqlServer);
            //var tests = context.GetAllHealth(DatabaseStoreType.SqlServer, "OR_M_Data_Entities.Tests.Tables");
            //if (test != null)
            //{

            //}

            //foreach (var health in tests)
            //{

            //}

            // after save, need to update the _tableOnLoad to match

            var items = context.From<Contact>();

            foreach (var item in items)
            {
                if (item != null)
                {

                }
            }

            var q = context.From<Contact>().Select(w => new Contact
            {
                ContactID = w.ContactID
            }).FirstOrDefault();

        }

        public class CS1 : CustomScript<Contact>
        {
            public int Id { get; set; }

            protected override string Sql
            {
                get { return @"

                    Select Top 1 * From Contacts Where Id = @Id

                "; }
            }
        }

        public class CS2 : CustomScript
        {
            public int Id { get; set; }

            public string Changed { get; set; }

            protected override string Sql
            {
                get { return @"

                    Update Contacts Set LastName = @Changed Where Id = @Id

                "; }
            }
        }

        [Script("GetLastName")]
        [ScriptPath("../../Scripts2")]
        public class SS1 : StoredScript<Contact>
        {
            public int Id { get; set; }
        }

        [ScriptAttribute("GetFirstName")]
        public class SS2 : StoredScript<Contact>
        {
        }

        [ScriptAttribute("UpdateFirstName")]
        public class SS3 : StoredScript
        {
            public string FirstName { get; set; }

            public int Id { get; set; }
        }

        [ScriptAttribute("GetFirstName")]
        public class SP1 : StoredProcedure<Contact>
        {
            public int Id { get; set; }
        }

        [Script("UpdateFirstName")]
        [Schema("dbo")]
        public class SP2 : StoredProcedure
        {
            [Index(1)]
            public int Id { get; set; }

            [Index(2)]
            public string FirstName { get; set; }
        }

        [ScriptAttribute("GetLastName")]
        public class SF1 : ScalarFunction<string>
        {
            [Index(1)]
            public int Id { get; set; }

            [Index(2)]
            public string FirstName { get; set; }
        }

        public class GetLastName2 : ScalarFunction<string>
        {

            public int Id { get; set; }
        }

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

            [DbType(SqlDbType.VarChar, "100")]
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

        public class User : EntityStateTrackable
        {
            public int ID { get; set; }

            public string Name { get; set; }
        }

        [Table("PhoneNumbers")]
        public class PhoneNumber : EntityStateTrackable
        {
            public int ID { get; set; }

            public string Phone { get; set; }

            public int PhoneTypeID { get; set; }

            [ForeignKey("PhoneTypeID")]
            public PhoneType PhoneType { get; set; }
        }

        public class PhoneType : EntityStateTrackable
        {
            public int ID { get; set; }

            public string Type { get; set; }
        }

        [Table("Appointments")]
        public class Appointment : EntityStateTrackable
        {
            [DbGenerationOption(DbGenerationOption.Generate)]
            public Guid ID { get; set; }

            public int ContactID { get; set; }

            public string Description { get; set; }

            public bool IsScheduled { get; set; }

            [ForeignKey("AppointmentID")]
            public List<Address> Address { get; set; }
        }

        public class Address : EntityStateTrackable
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

        public class StateCode : EntityStateTrackable
        {
            public int ID { get; set; }

            public string Value { get; set; }
        }

        [Table("ZipCode")]
        public class Zip : EntityStateTrackable
        {
            public int ID { get; set; }

            public string Zip5 { get; set; }

            public string Zip4 { get; set; }

            public int AddressID { get; set; }
        }

        public class Name : EntityStateTrackable
        {
            public int ID { get; set; }

            public string Value { get; set; }

            public int ContactID { get; set; }
        }
    }
}
