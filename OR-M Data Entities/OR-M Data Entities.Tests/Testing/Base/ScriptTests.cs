using System;
using System.Linq;
using OR_M_Data_Entities.Tests.StoredSql;
using OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff;

namespace OR_M_Data_Entities.Tests.Testing.Base
{
    public class ScriptTests
    {
        public static bool Test_1(DbSqlContext ctx)
        {
            try
            {
                var contact = ctx.ExecuteScript<Contact>(new CS1
                {
                    Id = 1
                }).FirstOrDefault();

                return contact != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_2(DbSqlContext ctx)
        {
            try
            {
                var nameChange = ctx.Find<Contact>(1);

                nameChange.FirstName = "NOTHING";

                ctx.SaveChanges(nameChange);

                ctx.ExecuteScript(new CS2
                {
                    Id = 1,
                    Changed = "NAME!"
                });

                var contact = ctx.Find<Contact>(1);

                return contact.ContactID == 1 && contact.LastName == "NAME!";
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_3(DbSqlContext ctx)
        {
            try
            {
                var contact = ctx.ExecuteScript<Contact>(new SS1
                {
                    Id = 1
                }).FirstOrDefault();

                return contact != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_4(DbSqlContext ctx)
        {
            try
            {
                var contacts = ctx.ExecuteScript<Contact>(new SS2()).ToList();

                return contacts.Any();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_5(DbSqlContext ctx)
        {
            try
            {
                var nameChange = ctx.Find<Contact>(1);

                nameChange.FirstName = "NOTHING";

                ctx.SaveChanges(nameChange);

                ctx.ExecuteScript(new SS3
                {
                    Id = 1,
                    FirstName = "DIFFERENT"
                });

                var contact = ctx.Find<Contact>(1);

                return contact.ContactID == 1 && contact.LastName == "DIFFERENT";
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_6(DbSqlContext ctx)
        {
            try
            {
                var contacts = ctx.ExecuteScript<Contact>(new SP1
                {
                    Id = 1
                }).ToList();

                return contacts.Any();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_7(DbSqlContext ctx)
        {
            try
            {
                var nameChange = ctx.Find<Contact>(1);

                nameChange.FirstName = "NOTHING";

                ctx.SaveChanges(nameChange);

                ctx.ExecuteScript(new SP2
                {
                    Id = 1,
                    FirstName = "DIFFERENT"
                });

                var contact = ctx.Find<Contact>(1);

                return contact.ContactID == 1 && contact.LastName == "DIFFERENT";
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
