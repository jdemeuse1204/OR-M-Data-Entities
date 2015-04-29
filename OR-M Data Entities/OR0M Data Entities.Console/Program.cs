using System.Collections.Generic;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Tests.Tables;

namespace OR0M_Data_Entities.Console
{
    class Program
    {
        static void Main2(string[] args)
        {
            var context = new DbSqlContext("sqlExpress");
            var ids = new List<int> {1, 2, 3};
            var result =
                context.From<Contact>().FirstOrDefault(w => ids.Contains(w.ID) && w.Number.Phone == "" && w.FirstName.Contains("James"));

            var list = new List<Contact>();
        }

        static void Main(string[] args)
        {
           Main2(args);return;

            //var context = new DbSqlContext("sqlExpress");

            //var lst = new List<string> { "James", "Megan" };

            //var orderedQuery =
            //    context.SelectAll<Contact>()
            //        .Where<Contact>(
            //            w =>
            //                DbFunctions.Convert(SqlDbType.VarChar, w.ID, null) ==
            //                DbFunctions.Convert(SqlDbType.VarChar, "", null))
            //        .ToList<Contact>();

            //if (orderedQuery != null)
            //{
                
            //}

            //var areader =
            //    context.Select<Person>(w => new
            //    {
            //        Name = w.FirstName,
            //        Trim = w.LastName
            //    })
            //    .Include<Car>(w => new
            //    {
            //        Test = w.ID
            //    })
            //        .InnerJoin<Person, Car>((person, car) => person.CarID == car.ID)
            //        .Where<Person>(w => w.FirstName.Equals("James"))
            //        .ToList<dynamic>();

            //var reader =
            //    context.SelectAll<Person>().Include<Car>()
            //        .InnerJoin<Person, Car>((person, car) => person.CarID == car.ID)
            //        .Where<Person>(w => w.FirstName.Equals("James"))
            //        .ToList<dynamic>();

            //var sreader =
            //    context.Select<Person>(w => w.FirstName)
            //        .InnerJoin<Person, Car>((person, car) => person.CarID == car.ID)
            //        .Where<Person>(w => w.FirstName.Equals("James"))
            //        .ToList<string>();

            //var treader =
            //    context.Select<Person>(w => new Car
            //    {
            //        Name = w.FirstName,
            //        Trim = w.LastName
            //    })
            //        .InnerJoin<Person, Car>((person, car) => person.CarID == car.ID)
            //        .Where<Person>(w => w.FirstName.Equals("James"))
            //        .ToList<Car>();

            //if (reader != null && treader != null && sreader != null && areader != null)
            //{

            //}

            //var val = context.ExecuteQuery<int>("Select 1 From Car").First();

            //if (val == 1)
            //{

            //}

            //// var parent = context.Find<Parent>(1);
            //var c = context.Find<Contact>(1);

            //if (c != null)
            //{

            //}

            //var name = context.Find<Name>(2);

            //var names = context.SelectAll<Name>().Where<Name>(w => w.ID == 7).ToList<Name>().Select(w => new Car { Name = w.Value });

            //if (name != null)
            //{

            //}

            ////if (parent != null)
            ////{

            ////}

            //var s = DateTime.Now;
            //var testItem = new Contact();
            ////context.From<Contact>()
            ////    .Select<Contact>()
            ////    .Where<Contact>(w => w.ID == 16)
            ////    .First<Contact>();

            ////context.Find<Contact>(16); 

            //var e = DateTime.Now;

            //var testSave = new Contact
            //{
            //    FirstName = "James",
            //    LastName = "Demeuse Just Added",
            //};

            //var testAppointment = new Appointment
            //{
            //    Description = "JUST ADDED APT!"
            //};

            //var testAddress = new Address
            //{
            //    Addy = "JUST ADDED!",
            //    State = new StateCode
            //    {
            //        Value = "MI"
            //    }
            //};

            //var testZip = new Zip
            //{
            //    Zip5 = "55416",
            //    Zip4 = "WIN!"
            //};

            //testAddress.ZipCode = new List<Zip>();
            //testAddress.ZipCode.Add(testZip);
            //testAppointment.Address = new List<Address> { testAddress };
            //testSave.Appointments = new List<Appointment>();
            //testSave.Name = null;
            ////    new List<Name>
            ////{ 
            ////    new Name
            ////    {
            ////        Value = "sldfljklsdf"
            ////    }
            ////};
            //testSave.Appointments.Add(testAppointment);

            //testSave.Number = new PhoneNumber
            //{
            //    Phone = "(414) 530-3086"
            //};

            //context.SaveChanges(testSave);

            //testItem =
            //    context.SelectAll<Contact>()
            //        .Where<Contact>(w => w.ID == testSave.ID)
            //        .First<Contact>();

            //context.Delete(testSave);

            //testItem =
            //   context.SelectAll<Contact>()
            //       .Where<Contact>(w => w.ID == testSave.ID)
            //       .First<Contact>();

            //var tt = e - s;

            //if (tt.Minutes != 0)
            //{

            //}

            //if (testItem != null)
            //{

            //}

            //var currentDateTime = DateTime.Now;

            //var totalMilliseconds = 0d;
            //var max = 1000;
            //var ct = 0;

            //for (var i = 0; i < max; i++)
            //{
            //    var start = DateTime.Now;
            //    var item = context.SelectAll<Contact>()
            //        .Where<Contact>(w => w.ID == 1)
            //        .First<Contact>();
            //    //    context.From<Policy>()
            //    //.Where<Policy>(w => DbFunctions.Cast(w.CreatedDate, SqlDbType.Date) == DbFunctions.Cast(currentDateTime, SqlDbType.Date))
            //    //.Select<Policy>()
            //    //.First<Policy>();

            //    if (item != null)
            //    {

            //    }

            //    var end = DateTime.Now;

            //    totalMilliseconds += (end - start).TotalMilliseconds;
            //    ct++;
            //}

            //var final = totalMilliseconds / ct;

            //if (final != 0)
            //{

            //}
        }
    }
}
