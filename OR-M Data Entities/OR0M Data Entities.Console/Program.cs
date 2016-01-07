using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Diagnostics.HealthMonitoring;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Scripts;
using OR_M_Data_Entities.Tests.Tables;

namespace OR0M_Data_Entities.Console
{
    public class SqlContext : DbSqlContext
    {
        public SqlContext() 
            : base("sqlExpress")
        {
            Configuration.UseTransactions = true;
            Configuration.ConcurrencyChecking.IsOn = true;
            Configuration.ConcurrencyChecking.ViolationRule = ConcurrencyViolationRule.OverwriteAndContinue;

            OnConcurrencyViolation += OnOnConcurrencyViolation;
        }

        private void OnOnConcurrencyViolation(object entity)
        {
            
        }
    }

    class Program
    {
        static bool Test(int i)
        {
            return false;
        }

        static void Main(string[] args)
        {
            var context = new SqlContext();
            var ids = new [] {1};
            var tests = new List<int> {1};
            //context.From<Contact>()
            //    .Where(
            //        w =>
            //            w.ContactID == 1 ||
            //            w.CreatedByUserID == 1 ||
            //            !w.FirstName.Equals("James")
            //            || w.LastName.Equals("WIN") == false
            //            && tests.Contains(w.ContactID)
            //            && w.EditedBy.Name.StartsWith("James") &&
            //            false == w.LastName.EndsWith("WIN")
            //            && !(w.ContactID > -1)
            //            && 1 >= w.ContactID);

            context.From<Contact>().Where(w => w.ContactID == w.Appointments.First(q => q.ID == Guid.Empty).ContactID);

            //var c1 = context.Find<Contact>(1);

            //c1.FirstName = "WINing!";

            //context.SaveChanges(c1);

            //var xy = new Contact
            //{
            //    FirstName = "James"
            //};

            //context.SaveChanges(xy);

            //var x = new Contact
            //{
            //    CreatedBy = new User
            //    {
            //        Name = "James Demeuse"
            //    },
            //    EditedBy = new User
            //    {
            //        Name = "Different User"
            //    },
            //    FirstName = "Test",
            //    LastName = "User",
            //    Names = new List<Name>
            //    {
            //        new Name
            //        {
            //            Value = "Win!"
            //        },
            //        new Name
            //        {
            //            Value = "FTW!"
            //        }
            //    },
            //    Number = new PhoneNumber
            //    {
            //        Phone = "555-555-5555",
            //        PhoneType = new PhoneType
            //        {
            //            Type = "Cell"
            //        }
            //    },
            //    Appointments = new List<Appointment>
            //    {
            //        new Appointment
            //        {
            //            Description = "Appointment 1",
            //            IsScheduled = false,
            //            Address = new List<Address>
            //            {
            //                new Address
            //                {
            //                    Addy = "1234 First Ave South",
            //                    State = new StateCode
            //                    {
            //                        Value = "MN"
            //                    },
            //                    ZipCode = new List<Zip>
            //                    {
            //                        new Zip
            //                        {
            //                            Zip4 = "5412",
            //                            Zip5 = "55555"
            //                        },
            //                        new Zip
            //                        {
            //                            Zip5 = "12345"
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //};

            var x = context.Find<Contact>(2077);

            x.FirstName = "WINss";

            context.Delete(x);

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

    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);

            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
