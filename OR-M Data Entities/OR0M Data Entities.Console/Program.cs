using System;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Tests.Tables;

namespace OR0M_Data_Entities.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new DbSqlContext("sqlExpress");

            var contact = new Contact
            {
                FirstName = "James",
                LastName = "Demeuse",
                Names = new List<Name>()
                {
                    new Name
                    {
                        Value = "NEW"
                    }
                },
                Number = new PhoneNumber
                {
                    Phone = "414",
                    PhoneType = new PhoneType
                    {
                        Type = "CELL"
                    }
                },
                CreatedBy = new User
                {
                    ID  = 1,
                    Name = "James"
                },
                EditedBy = new User
                {
                    ID = 1,
                    Name = "James"
                },
                EditedByUserID = 1,
                CreatedByUserID = 1,
                Appointments = new List<Appointment>
                {
                    new Appointment
                    {
                        Description = "Date Added!",
                        Address = new List<Address>
                        {
                            new Address
                            {
                                Addy = "MY ADDY!",
                                State = new StateCode
                                {
                                    Value = "MN"
                                },
                                ZipCode = new List<Zip>
                                {
                                    new Zip
                                    {
                                        Zip4 = "0000",
                                        Zip5 = "55416"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var saveType = context.SaveChanges(contact);

            if (saveType == ChangeStateType.Insert)
            {

            }

            var lst = new List<string>() {"james", "megan"};

            var s = DateTime.Now;
            var c = new Contact();
                //context.FromView<Contact>("ContactOnly")
                //    .Where(w => w.FirstName == "James")
                //    .Select(w => w.Number.Phone)
                //    .ToList();
            var e = DateTime.Now;
            var f = e - s;

            if (c != null && f.Days != 0)
            {

            }

            s = DateTime.Now;
            var a =
                context.From<Contact>().First(w => w.ID == 2 && w.Appointments.Any(x => x.Description == "James"));

            e = DateTime.Now;
            f = e - s;

            if (a != null && f.Days != 0)
            {

            }
            //context.Delete(contact);
        }
    }
}
