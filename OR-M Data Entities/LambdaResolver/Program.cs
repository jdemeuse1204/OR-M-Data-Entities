using System;
using System.Collections.Generic;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Tests.Tables;

namespace LambdaResolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var ctx = new DbSqlContext("sqlExpress");

            var lst = new List<int> { 1, 2, 3, 4, 5 };
            var s = DateTime.Now;
            var item = ctx.From<Contact>().Where(
                w =>
                    w.ID == 1 &&
                    w.FirstName == "James" ||
                    w.FirstName == "Megan" &&
                    w.FirstName == "WIN" &&
                    w.FirstName == "AHHHH" ||
                    w.FirstName == "" &&
                    !w.FirstName.StartsWith("Coolness") &&
                    w.Number.PhoneType.Type == "Home" &&
                    lst.Contains(w.ID) &&
                    w.LastName == ctx.From<Appointment>().Where(x => x.ContactID == 1).Select(x => x.Description).FirstOrDefault()
                    )
                .InnerJoin(ctx.From<Appointment>(), contact => contact.ID, appointment => appointment.ContactID,
                    (contact, appointment) => new
                    {
                        contact.ID,
                        LastName = contact.FirstName,
                        FirstName = contact.LastName,
                        contact.Name,
                        contact.Number,
                        contact.PhoneID,
                        appointment
                    })
                .InnerJoin(ctx.From<PhoneNumber>(), contact => contact.PhoneID, number => number.ID,
                    (contact, number) => contact.ID).FirstOrDefault();

            if (item != null)
            {

            }
            var e = DateTime.Now;

            var f = e - s;

            if (f.Days == 1)
            {

            }

            s = DateTime.Now;

            //item = ctx.From<Contact>().Where(
            //    w =>
            //        w.ID == 1 && w.FirstName == "James" ||
            //        w.FirstName == "Megan" && w.FirstName == "WIN" && w.FirstName == "AHHHH" ||
            //        w.FirstName == "" && !w.FirstName.StartsWith("Coolness") && w.Number.PhoneType.Type == "Home")
            //    .InnerJoin(ctx.From<PhoneNumber>(), contact => contact.PhoneID, number => number.ID,
            //        (contact, number) => new Contact
            //        {
            //            ID = contact.ID
            //        });
            e = DateTime.Now;

            f = e - s;
            //.Select(w => new Contact
            //{
            //    ID = w.ID,
            //    Appointments = ctx.From<Appointment>().Where(x => x.ContactID == w.ID).All(),
            //    FirstName = w.FirstName,
            //    LastName = w.LastName,
            //    Name = w.Name,
            //    Number = w.Number,
            //    PhoneID = w.PhoneID
            //});  


            //.InnerJoin(ctx.From<Appointment>(), contact => contact.ID, appointment => appointment.ContactID,
            //    (contact, appointment) => new Contact
            //    {
            //        ID = contact.ID,
            //        Appointments = new List<Appointment>{appointment},
            //        FirstName = contact.FirstName,
            //        LastName = contact.LastName,
            //        Name = contact.Name,
            //        Number = contact.Number,
            //        PhoneID = contact.PhoneID
            //    })
            //.InnerJoin(ctx.From<PhoneNumber>(), contact => contact.PhoneID, number => number.ID,
            //    (contact, number) => contact);
            //w.ID == ctx.From<Appointment>().Where(z => z.Description == "").Select(x => x.ContactID).First());
        }
    }
}
