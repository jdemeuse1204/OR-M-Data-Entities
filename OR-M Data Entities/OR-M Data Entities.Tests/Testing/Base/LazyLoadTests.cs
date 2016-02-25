using OR_M_Data_Entities.Tests.Tables;
using OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff;

namespace OR_M_Data_Entities.Tests.Testing.Base
{
    public static class LazyLoadTests
    {
        public static bool Test_1(DbSqlContext ctx)
        {
            var contact = ctx.From<Contact>().Where(w => w.ContactID == 1).Include("Users").First();

            return false;
        }
    }
}
