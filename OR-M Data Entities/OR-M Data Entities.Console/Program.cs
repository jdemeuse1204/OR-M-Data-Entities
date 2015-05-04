using OR_M_Data_Entities.Tests.Tables;

namespace OR_M_Data_Entities.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new DbSqlContext("sqlExpress");

            var contact = context.Where<Contact>(w => w.FirstName == null).InnerJoin<Contact, Appointment>((c,a) => c.ID == a.ContactID).First<Contact>();

            if (contact != null)
            {
                
            }
        }
    }
}
