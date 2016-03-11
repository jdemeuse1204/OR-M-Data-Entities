using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using OR_M_Data_Entities.Tests.Tables;
using OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff;


namespace OR_M_Data_Entities.Compare
{
    class Program
    {
        static void Main(string[] args)
        {
            var efCtx = new EntityFrameworkContext();

            var s = DateTime.Now;
            //var tt = context.Find<Contact>(500);
            //var t = context.From<Contact>().Where(w => w.ContactID == 1).Select(w => new
            //{
            //    ID = w.ContactID,
            //    w.CreatedByUserID,
            //    w.FirstName,
            //    w.Number,
            //    w.Appointments,
            //    w.Number.PhoneType
            //}).ToList();
            var c = efCtx.Contacts.FirstOrDefault(w => w.ContactID == 1);

            var ttt =
                efCtx.Contacts.Include("Appointments")
                    .Include("Appointments.Address")
                    .Include("Names")
                    .Include("PhoneNumbers")
                    .Include("PhoneNumbers.PhoneType")
                    .Include("Appointments.Address.State")
                    .Include("CreatedByUser")
                    .Include("EditedByUser")
                    .Include("Appointments.Address.Zip")
                    .ToList().OrderBy(w => w.ContactID);

            var e = DateTime.Now;

            System.Console.WriteLine((e - s).Milliseconds);
        }
    }
}
