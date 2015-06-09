using System;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Tests.Tables;

namespace OR0M_Data_Entities.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new DbSqlContext("sqlExpress");

            var s = DateTime.Now;
            var e = DateTime.Now;
            var f = e - s;

            var result2 = context.From<Policy>().Select(w => w.StateID).Max();

            e = DateTime.Now;
            f = e - s;

            if (f.Days == 1)
            {

            }

            s = DateTime.Now;
            var result =
                context.From<Contact>()
                    .Where(
                        w =>
                            w.ID == 2 &&
                            w.FirstName ==
                            context.From<Appointment>()
                                .Where(x => x.ContactID == 2 && x.Description == "James")
                                .Select(x => x.Description)
                                .FirstOrDefault());
            e = DateTime.Now;

            f = e - s;

            if (f.Days == 1)
            {

            }

            if (result != null && result2 != null)
            {

            }
        }
    }
}
