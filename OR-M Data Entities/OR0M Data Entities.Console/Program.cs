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

            for (int i = 0; i < 100; i++)
            {
                var v = context.ExecuteScript<Contact>(new CS1
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
