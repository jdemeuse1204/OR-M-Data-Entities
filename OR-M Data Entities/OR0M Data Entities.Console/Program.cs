using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Diagnostics.HealthMonitoring;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Scripts;
using OR_M_Data_Entities.Tests.Tables;

namespace OR0M_Data_Entities.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new DbSqlContext("sqlExpress");

            //var c1 = context.Find<Contact>(1);

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

            var result = context.SaveChanges(x);

            if (result != null)
            {

            }

            x.FirstName = "New Name";

            result = context.SaveChanges(x);

            var test = result.GetUpdateType("Contacts");

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


            for (int i = 0; i < 100; i++)
            {
                var v = context.ExecuteScript<Contact>(new SS1
                {
                    Id = 1
                }).ToList();
            }

            var c = context.Find<Contact>(1);

            if (c != null)
            {

            }

            var user = context.Find<User>(1);
            var user2 = context.Find<User>(2);

            var contact = new Contact
            {
                CreatedBy = user,
                CreatedByUserID = user.ID,
                EditedBy = user2,
                EditedByUserID = user2.ID,
                FirstName = "James",
                LastName = "Demeuse"
            };

            context.SaveChanges(contact);
        }

        public class CS1 : CustomScript<Contact>
        {
            public int Id { get; set; }

            protected override string Sql
            {
                get
                {
                    return @"

                    Select Top 1 * From Contacts Where Id = @Id

                ";
                }
            }
        }

        public class CS2 : CustomScript
        {
            public int Id { get; set; }

            public string Changed { get; set; }

            protected override string Sql
            {
                get
                {
                    return @"

                    Update Contacts Set LastName = @Changed Where Id = @Id

                ";
                }
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
    }
}
