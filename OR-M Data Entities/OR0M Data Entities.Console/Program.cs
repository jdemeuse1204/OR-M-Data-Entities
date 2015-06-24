using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Commands.Transform;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Tests.Tables;

namespace OR0M_Data_Entities.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new DbSqlContext("sqlExpress");
            //context.Delete()
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

            //var saveType = context.SaveChanges(contact);

            //if (saveType == ChangeStateType.Insert)
            //{

            //}

            var lst = new List<string>() {"james", "megan"};

            var s = DateTime.Now;
            var c =
                context.From<Policy>()
                    .InnerJoin(context.From<PolicyType>(), policy => policy.PolicyInfoId, type => type.ID,
                        (policy, type) => new { policy.Id })
                    .ToList();
            var e = DateTime.Now;
            var f = e - s;

            if (c != null && f.Days != 0)
            {

            }

            s = DateTime.Now;
            var a =
                context.From<Contact>()
                    .First(
                        w =>
                            DbTransform.Convert(SqlDbType.BigInt, 1, 1)
                                == DbTransform.Convert(SqlDbType.Float, w.ID, 1) &&
                            w.Appointments.Any(
                                x =>
                                    DbTransform.Cast(x.Description, SqlDbType.VarChar) ==
                                    DbTransform.Cast("JAMES", SqlDbType.VarChar)));

            e = DateTime.Now;
            f = e - s;

            if (a != null && f.Days != 0)
            {

            }
            //context.Delete(contact);
        }
    }
}
