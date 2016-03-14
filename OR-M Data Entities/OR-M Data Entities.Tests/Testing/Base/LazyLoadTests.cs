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
                var contact = ctx.From<Contact>().Where(w => w.ContactID == 1).Include("Appointments").First();

                return contact != null && contact.Appointments.All(w => w.ID != Guid.Empty);
            }
            catch
            {
                return false;
            }
        }

        public static bool Test_2(DbSqlContext ctx)
        {
            // make sure include works
            try
            {
                var contact = ctx.From<Contact>().Where(w => w.ContactID == 1).Include("Appointments.Address.ZipCode").First();

                return contact != null && contact.Appointments.All(w => w.ID != Guid.Empty) &&
                       contact.Appointments.All(w => w.Address.All(x => x.ZipCode.All(y => y.ID != 0)));
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
                var contact = ctx.From<Contact>().Where(w => w.ContactID == 1).IncludeAll().First();

                return contact.EditedBy.ID != 0 && contact.CreatedByUserID != 0 && contact.Names.All(w => w.ID != 0) &&
                       contact.Number.ID != 0 && contact.Number.PhoneTypeID != 0 &&
                       contact.Appointments.All(w => w.ID != Guid.Empty) &&
                       contact.Appointments.All(w => w.Address.All(x => x.ID != 0)) &&
                       contact.Appointments.All(w => w.Address.All(x => x.ZipCode.All(y => y.ID != 0))) &&
                       contact.Appointments.All(w => w.Address.All(x => x.State.ID != 0));
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
                var contact = ctx.From<Contact>().Where(w => w.ContactID == 1).Include("Appointments.Address.ZipCode").First();

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
                        .Include("EditedBy")
                        .Include("CreatedBy")
                        .Include("Appointments")
                        .First();

                return contact.EditedBy.ID != 0 && contact.CreatedBy.ID != 0 && contact.Appointments.All(w => w.ID != Guid.Empty);
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
