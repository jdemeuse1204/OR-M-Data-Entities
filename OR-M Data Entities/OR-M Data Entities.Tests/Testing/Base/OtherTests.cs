using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Tests.Tables;
using OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff;
using OR_M_Data_Entities.Tests.Tables.Other;
using OR_M_Data_Entities.Tests.Testing.Context;

namespace OR_M_Data_Entities.Tests.Testing.Base
{
    public static class OtherTests
    {
        public static bool Test_1(InsertKeyChangeContext ctx)
        {
            try
            {
                var policy = new Policy
                {
                    Id = -1,
                    County = "Hennepin",
                    CreatedBy = "James Demeuse",
                    CreatedDate = DateTime.Now,
                    FeeOwnerName = "Test",
                    FileNumber = 100,
                    InsuredName = "James Demeuse",
                    PolicyAmount = 100,
                    PolicyDate = DateTime.Now,
                    PolicyInfoId = 1,
                    PolicyNumber = "001-8A",
                    StateID = 7,
                    UpdatedBy = "Me",
                    UpdatedDate = DateTime.Now
                };

                ctx.SaveChanges(policy);

                return policy.Id != -1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_2(InsertKeyChangeContext ctx)
        {
            try
            {
                var item = new StringKeyTest
                {
                    Id = "COOL",
                    Value = "JAMES!"
                };

                ctx.SaveChanges(item);

                return item.Id == "COOL";
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_3(TransactionContext ctx)
        {
            // test with EST on and off
            try
            {
                var id = ctx.From<Contact>().Select(w => w.ContactID).Max();
                var c1 = ctx.Find<Contact>(id);

                c1.Appointments.Add(new OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff.Appointment
                {
                    Description = "AppointmentING!STUFF",
                    IsScheduled = false,
                    Address = new List<Address>
                    {
                        new Address
                        {
                            Addy = "Some Street",
                            State = new StateCode
                            {
                                Value = "MN"
                            },
                            ZipCode = new List<Zip>
                            {
                                new Zip
                                {
                                    Zip4 = "5412",
                                    Zip5 = "55555"
                                },
                                new Zip
                                {
                                    Zip5 = "12345"
                                }
                            }
                        }
                    }
                });

                ctx.SaveChanges(c1);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_4(TransactionContext ctx)
        {
            // test with EST on and off
            try
            {
                var c1 = ctx.Find<Tables.EntityStateTrackableOn.Contact>(1);

                c1.Appointments.Add(new Tables.EntityStateTrackableOn.Appointment
                {
                    Description = "AppointmentING!STUFF",
                    IsScheduled = false,
                    Address = new List<Tables.EntityStateTrackableOn.Address>
                    {
                        new Tables.EntityStateTrackableOn.Address
                        {
                            Addy = "Some Street",
                            State = new Tables.EntityStateTrackableOn.StateCode
                            {
                                Value = "MN"
                            },
                            ZipCode = new List<Tables.EntityStateTrackableOn.Zip>
                            {
                                new Tables.EntityStateTrackableOn.Zip
                                {
                                    Zip4 = "5412",
                                    Zip5 = "55555"
                                },
                                new Tables.EntityStateTrackableOn.Zip
                                {
                                    Zip5 = "12345"
                                }
                            }
                        }
                    }
                });

                ctx.SaveChanges(c1);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_5(DefaultContext ctx)
        {
            // test linked server join, should not optimize this join because its
            // from one linked server to another linked server table
            try
            {
                var c1 = ctx.Find<Order>(1);

                return false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public static bool Test_6(DefaultContext ctx)
        {
            // test linked server join, should optimize this join because its
            // across servers
            try
            {
                var c1 = ctx.Find<Test>(1);

                return false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public static bool Test_7(DefaultContext ctx)
        {
            // make sure we can insert guid with dbdefault
            try
            {
                var t = new DbDefaultGuidTest
                {
                    Test = "WIN!"
                };

                ctx.SaveChanges(t);

                return t.Id != Guid.Empty;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_8(LazyLoadContext ctx)
        {
            // make sure we can insert guid with dbdefault
            try
            {
                var id = ctx.From<Contact>().Select(w => w.ContactID).Max();
                var contact =
                    ctx.From<Contact>()
                        .Include("Appointments")
                        .Include("Appointments.Address")
                        .Include("Appointments.Address.State")
                        .Include("Appointments.Address.ZipCode")
                        .FirstOrDefault(w => w.ContactID == id);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_9(LazyLoadContext ctx)
        {
            // should be able to update without any FK's present
            try
            {
                var id = ctx.From<Contact>().Select(w => w.ContactID).Max();
                var contact = ctx.From<Contact>().FirstOrDefault(w => w.ContactID == id);

                ctx.SaveChanges(contact);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_10(DefaultContext ctx)
        {
            // make sure the return override is reset after doing a select
            try
            {
                var id = ctx.From<Contact>().Select(w => w.ContactID).Max();
                var contact = ctx.From<Contact>().FirstOrDefault(w => w.ContactID == id);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
