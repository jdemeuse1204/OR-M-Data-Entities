using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Commands.Transform;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Scripts;
using OR_M_Data_Entities.Tests.Tables;

namespace OR0M_Data_Entities.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // after save, need to update the _tableOnLoad to match
            var context = new DbSqlContext("sqlExpress");

            context.ExecuteScript(new CS2
            {
                Id = 10,
                Changed = "LastName"
            });

            context.ExecuteScript(new SS3
            {
                Id = 10,
                FirstName = "Tony"
            });

            var reader = context.ExecuteScript<Contact>(new CS1
            {
                Id = 10
            }).First();

            reader = context.ExecuteScript<Contact>(new SS1
            {
                Id = 10
            }).First();

            reader = context.ExecuteScript<Contact>(new SS2()).First();

            context.ExecuteScript(new SP2
            {
                Id = 10,
                FirstName = "Megan"
            });

            reader = context.ExecuteScript<Contact>(new SP1
            {
                Id = 10
            }).First();

            var name = context.ExecuteScript<string>(new SF1
            {
                Id = 10,
                FirstName = "Megan"
            }).First();

            name = context.ExecuteScript<string>(new GetLastName2
            {
                Id = 10
            }).First();

            if (reader != null && name != null)
            {
                
            }

            var appointment = new Appointment();
            appointment.ContactID = 10;
            appointment.Description = "NEW";
            appointment.IsScheduled = false;

            //context.Delete(appointment);

            var policy = context.Find<Contact>(10);

            // Change first or default/first?  do select top 1 (primary key(s)) to get the keys, then to select for first /firstdefault?


            context.SaveChanges(policy);

            var entityState = policy.GetState();

            policy.FirstName = "CHANGED!";

            entityState = policy.GetState();

            if (entityState == EntityState.Modified)
            {
                context.SaveChanges(policy);

            }


            entityState = policy.GetState();
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

        [Script("GetFirstName")]
        public class SS2 : StoredScript<Contact>
        {
        }

        [Script("UpdateFirstName")]
        public class SS3 : StoredScript
        {
            public string FirstName { get; set; }

            public int Id { get; set; }
        }

        [Script("GetFirstName")]
        public class SP1 : StoredProcedure<Contact>
        {
            public int Id { get; set; }
        }

        [Script("UpdateFirstName")]
        [ScriptSchema("dbo")]
        public class SP2 : StoredProcedure
        {
            [Index(1)]
            public int Id { get; set; }

            [Index(2)]
            public string FirstName { get; set; }
        }

        [Script("GetLastName")]
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
