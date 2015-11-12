using System;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Tests.Tables;


namespace OR_M_Data_Entities.Compare
{
    class Program
    {
        static void Main(string[] args)
        {
            var dsf = string.Format("-0{0}:00", Math.Abs(DateTimeOffset.Now.Offset.Hours));

            if (dsf != null)
            {
                
            }

            var s1 = DateTime.Now;
            using (var efCtx = new EntityFrameworkContext())
            {

                var c1 = efCtx.Contacts.ToList();

                List<EFTables.Contact> test = c1;

                foreach (var contact in test)
                {
                    
                }

                var result = c1.Select(contact => new EFTables.Contact
                {
                    CreatedByUser = contact.CreatedByUser,
                    EditedByUser = contact.EditedByUser,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    PhoneNumbers = contact.PhoneNumbers,
                    Appointments = contact.Appointments
                }).ToList();

                var e1 = DateTime.Now;
                var d1 = e1 - s1;

                if (d1.TotalDays != null && result != null)
                {

                }

                if (c1 != null)
                {

                }
            }

            var s2 = DateTime.Now;
            using (var ctx = new ORMDEContext())
            {
                var c2 = ctx.From<Contact>().Where(w => w.FirstName.Contains("Win")).Count(w => w.ContactID == 10);

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
