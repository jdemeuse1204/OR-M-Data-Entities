using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Tests.Context;
using OR_M_Data_Entities.Tests.Tables;

namespace OR0M_Data_Entities.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new SqlContext();
            var s = DateTime.Now;
            var testItem =
                context.From<Contact>()
                    .Select<Contact>()
                    .Where<Contact>(w => w.ID == 16)
                    .First<Contact>();

            var e = DateTime.Now;

            var testSave = new Contact
            {
                FirstName = "James",
                LastName = "Demeuse Just Added",
            };

            var testAppointment = new Appointment
            {
                Description = "JUST ADDED APT!"
            };

            var testAddress = new Address
            {
                Addy = "JUST ADDED!"
            };

            var testZip = new Zip
            {
                Zip5 = "55416",
                Zip4 = "WIN!"
            };

             testAddress.ZipCode = new List<Zip>();
            testAddress.ZipCode.Add(testZip);
            testAppointment.Address = testAddress;
            testSave.Appointments = new List<Appointment>();
            testSave.Name = new Name
            {
                Value = "sldfljklsdf"
            };
            testSave.Appointments.Add(testAppointment);

            context.SaveChanges(testSave);

            testItem =
                context.From<Contact>()
                    .Select<Contact>()
                    .Where<Contact>(w => w.ID == testSave.ID)
                    .First<Contact>();

            var tt = e - s;

            if (tt.Minutes != 0)
            {
                
            }

            if (testItem != null)
            {

            }

            var currentDateTime = DateTime.Now;

            var totalMilliseconds = 0d;
            var max = 1000;
            var ct = 0;

            for (var i = 0; i < max; i++)
            {
                var start = DateTime.Now;
                var item = context.From<Contact>()
                    .Select<Contact>()
                    .Where<Contact>(w => w.ID == 16)
                    .First<Contact>();
                //    context.From<Policy>()
                //.Where<Policy>(w => DbFunctions.Cast(w.CreatedDate, SqlDbType.Date) == DbFunctions.Cast(currentDateTime, SqlDbType.Date))
                //.Select<Policy>()
                //.First<Policy>();

                if (item != null)
                {

                }

                var end = DateTime.Now;

                totalMilliseconds += (end - start).TotalMilliseconds;
                ct++;
            }

            var final = totalMilliseconds / ct;

            if (final != 0)
            {

            }
        }
    }
}
