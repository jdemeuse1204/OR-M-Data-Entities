using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Data.PayloadOperations;
using OR_M_Data_Entities.Tests.Context;
using OR_M_Data_Entities.Tests.Tables;

namespace OR0M_Data_Entities.Console
{
    class Program
    {
        static void Main(string[] args)
        {

            var x = new ObjectSelectBuilder();
            //x.Select<Parent>();
            x.SelectAll<Zip>();
            x.AddLeftJoin<Zip, Address>((p, c) => p.AddressID == c.ID);
            x.Build();
            var context = new SqlContext();
            var reader = context.ExecuteQuery<Zip>(x);
            var itemX = reader.Select();

            var parent = context.Find<Parent>(1);

            var name = context.Find<Name>(2);

            name = context.From<Name>().Where<Name>(w => w.ID == 7).Select<Name>().First<Name>();

            if (name != null)
            {
                
            }

            if (parent != null)
            {
                
            }

            var s = DateTime.Now;
            var testItem = context.Find<Contact>(1); //new Contact();
            //context.From<Contact>()
            //    .Select<Contact>()
            //    .Where<Contact>(w => w.ID == 16)
            //    .First<Contact>();

            //context.Find<Contact>(16); 

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
                Addy = "JUST ADDED!",
                State = new StateCode
                {
                    Value = "MI"
                }
            };

            var testZip = new Zip
            {
                Zip5 = "55416",
                Zip4 = "WIN!"
            };

            testAddress.ZipCode = new List<Zip>();
            testAddress.ZipCode.Add(testZip);
            testAppointment.Address = new List<Address> { testAddress };
            testSave.Appointments = new List<Appointment>();
            testSave.Name = new List<Name>
            { 
                new Name
                {
                    Value = "sldfljklsdf"
                }
            };
            testSave.Appointments.Add(testAppointment);

            testSave.Number = new PhoneNumber
            {
                Phone = "(414) 530-3086"
            };

            context.SaveChanges(testSave);

            testItem =
                context.From<Contact>()
                    .Select<Contact>()
                    .Where<Contact>(w => w.ID == testSave.ID)
                    .First<Contact>();

            context.Delete(testSave);

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
