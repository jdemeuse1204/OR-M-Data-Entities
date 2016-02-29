using System;
using System.Linq;
using OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff;

namespace OR_M_Data_Entities.Tests.Testing.Base
{
    public static class LazyLoadTests
    {
        public static bool Test_1(DbSqlContext ctx)
        {
            // should throw an error if the table is not found
            try
            {
                var contact = ctx.From<Contact>().Where(w => w.ContactID == 1).Include("Users").First();

                return false;
            }
            catch
            {
                return true;
            }
        }

        public static bool Test_2(DbSqlContext ctx)
        {
            // make sure include works
            try
            {
                var contact = ctx.From<Contact>().Where(w => w.ContactID == 1).Include("User").First();

                return contact != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_3(DbSqlContext ctx)
        {
            // make sure include works when there are two tables of the same name,
            // should be able to use selector
            try
            {
                var contact = ctx.From<Contact>().Where(w => w.ContactID == 1).Include("User[EditedBy]").First();

                return contact.EditedBy != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_4(DbSqlContext ctx)
        {
            // test include to
            try
            {
                var contact = ctx.From<Contact>().Where(w => w.ContactID == 1).IncludeTo("ZipCode").First();

                return contact.Appointments.Any(w => w.Address.Any(x => x.ZipCode.Any()));
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_5(DbSqlContext ctx)
        {
            // test include all
            try
            {
                var contact = ctx.From<Contact>().Where(w => w.ContactID == 1).IncludeAll().First();

                return contact.EditedBy != null && contact.CreatedBy != null && contact.Appointments.Any() &&
                       contact.Number != null && contact.Names.Any() && contact.Number.PhoneType != null
                       && contact.Appointments.Any(w => w.Address.Any()) && contact.Appointments.Any(w => w.Address.Any(x => x.ZipCode.Any()))
                       && contact.Appointments.Any(w => w.Address.Any(x => x.State != null));
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_6(DbSqlContext ctx)
        {
            // multiple includes must work
            try
            {
                var contact =
                    ctx.From<Contact>()
                        .Where(w => w.ContactID == 1)
                        .Include("User[EditedBy]")
                        .Include("User[CreatedBy]")
                        .Include("Appointments")
                        .First();

                return contact.EditedBy != null && contact.CreatedBy != null && contact.Appointments.Any();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_7(DbSqlContext ctx)
        {
            // parent must be included, query should fail
            try
            {
                var contact =
                    ctx.From<Contact>()
                        .Where(w => w.ContactID == 1)
                        .Include("ZipCode")
                        .First();

                return false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }
    }
}
