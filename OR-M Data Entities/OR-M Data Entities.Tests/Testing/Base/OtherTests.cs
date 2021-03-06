﻿using System;
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
                var c1 = ctx.Find<Contact>(1);

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
    }
}
