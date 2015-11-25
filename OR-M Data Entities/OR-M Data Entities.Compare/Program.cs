using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Tests.Tables;


namespace OR_M_Data_Entities.Compare
{
    class Program
    {
        static void Main(string[] args)
        {
            var s1 = DateTime.Now;
            using (var efCtx = new EntityFrameworkContext())
            {
                var z = efCtx.Contacts.FirstOrDefault();

                z.Appointments.FirstOrDefault().Description = "fghfgh";

                efCtx.SaveChanges();

                var e1 = DateTime.Now;
                var d1 = e1 - s1;

                if (d1.TotalDays != null && z != null)
                {

                }
            }

            var s2 = DateTime.Now;
            using (var ctx = new ORMDEContext())
            {
                var c2 = ctx.From<Contact>().FirstOrDefault(w => w.ContactID == 1);

                var e2 = DateTime.Now;
                var d2 = e2 - s2;
                

                if (d2.TotalDays != null)
                {

                }

                if (c2 != null)
                {

                }
            }
        }
    }
}
