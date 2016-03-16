using System.Linq;
using OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff;

namespace OR_M_Data_Entities.Tests.Testing.Base
{
    public class LambdaResolverTests
    {
        public static bool Test_1(DbSqlContext ctx)
        {
            var t = ctx.From<Appointment>().FirstOrDefault(w => w.IsScheduled == false && w.ContactID == 500);

            return t != null;
        }

        public static bool Test_2(DbSqlContext ctx)
        {
            var t = ctx.From<Appointment>().FirstOrDefault(w => w.IsScheduled && w.ContactID == 500);

            return t == null;
        }

        public static bool Test_3(DbSqlContext ctx)
        {
            var t = ctx.From<Appointment>().FirstOrDefault(w => !w.IsScheduled.Equals(false) && w.ContactID == 500);

            return t == null;
        }

        public static bool Test_4(DbSqlContext ctx)
        {
            var t = ctx.From<Appointment>().FirstOrDefault(w => !w.IsScheduled && w.ContactID == 500);

            return t != null;
        }

        public static bool Test_5(DbSqlContext ctx)
        {
            var t = ctx.From<Appointment>().FirstOrDefault(w => w.IsScheduled.Equals(true) && w.ContactID == 500);

            return t == null;
        }

        public static bool Test_6(DbSqlContext ctx)
        {
            var compare = false;

            var t =
                ctx.From<Appointment>()
                    .FirstOrDefault(
                        w =>
                            w.IsScheduled.Equals(true) && w.IsScheduled && !w.IsScheduled &&
                            compare.Equals(w.IsScheduled) && !w.IsScheduled.Equals(true));

            return true;
        }

        public static bool Test_7(DbSqlContext ctx)
        {
            var id = 1;

            var t =
                ctx.From<Contact>()
                    .FirstOrDefault(
                        w =>
                            w.ContactID.Equals(id) && w.Appointments.Any(x => x.IsScheduled));

            return true;
        }

        public static bool Test_8(DbSqlContext ctx)
        {
            var id = 1;

            var t =
                ctx.From<Contact>()
                    .FirstOrDefault(
                        w =>
                            w.ContactID.Equals(id) && w.Appointments.Any(x => x.IsScheduled == false));

            return true;
        }

        public static bool Test_9(DbSqlContext ctx)
        {
            var t =
                ctx.From<Contact>()
                    .Where(w => w.ContactID == 1)
                    .Include("Appointments")
                    .Select(w => w.Appointments)
                    .ToList();

            return true;
        }

        public static bool Test_10(DbSqlContext ctx)
        {
            var t =
                ctx.From<Contact>()
                    .Where(w => w.ContactID == 1)
                    .Include("Appointments")
                    .Select(w => w.Appointments)
                    .FirstOrDefault();

            return true;
        }
    }
}
