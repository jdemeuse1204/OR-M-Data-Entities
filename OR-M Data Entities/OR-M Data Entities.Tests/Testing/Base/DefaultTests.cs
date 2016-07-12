using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Tests.StoredSql;
using OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn;

namespace OR_M_Data_Entities.Tests.Testing.BaseESTOn
{
    using Tables.EntityStateTrackableOn;

    public class DefaultTestsESTOn
    {
        public static bool Test_Extra_1(DbSqlContext ctx)
        {
            // test entity state
            var phoneType = new PhoneType
            {
                Type = "Cell"
            };

            var beforeSave = phoneType.GetState();

            ctx.SaveChanges(phoneType);

            var afterSave = phoneType.GetState();

            return (beforeSave == EntityState.Modified && afterSave == EntityState.UnChanged);
        }

        public static bool Test_Extra_2(DbSqlContext ctx)
        {
            // Make sure entity state tracking is working properly
            var policy = new Policy();

            var state0 = policy.GetState();

            policy.County = "Hennepin";
            policy.CreatedBy = "James Demeuse";
            policy.CreatedDate = DateTime.Now;
            policy.FeeOwnerName = "Test";
            policy.FileNumber = 100;
            policy.InsuredName = "James Demeuse";
            policy.PolicyAmount = 100;
            policy.PolicyDate = DateTime.Now;
            policy.PolicyInfoId = 1;
            policy.PolicyNumber = "001-8A";
            policy.StateID = 7;
            policy.UpdatedBy = "Me";
            policy.UpdatedDate = DateTime.Now;

            var state1 = policy.GetState();

            ctx.SaveChanges(policy);

            var state2 = policy.GetState();

            policy = ctx.Find<Policy>(policy.Id);

            var state3 = policy.GetState();

            policy.InsuredName = Guid.NewGuid().ToString();

            var state4 = policy.GetState();

            return (state0 == EntityState.Modified && state1 == EntityState.Modified &&
                          state2 == EntityState.UnChanged && state3 == EntityState.UnChanged &&
                          state4 == EntityState.Modified);
        }

        public static bool Test_Extra_3(DbSqlContext ctx)
        {
            try
            {
                var id = ctx.From<History>().Select(w => w.Id).Max();

                // Should get errors when we try to set a value in
                // a timestamp column with an update when entity state
                // tracking is on
                var history = ctx.Find<History>(id);

                history.CreateDate = new byte[] { 0, 0, 0, 0, 68, 30 };

                ctx.SaveChanges(history);

                return false;
            }
            catch (SqlSaveException)
            {
                return true;
            }
        }

        public static bool Test_1(DbSqlContext ctx)
        {
            // Disconnnect Test With Execute Query
            ctx.ExecuteQuery("Select Top 1 1");

            var state = ctx.GetConnectionState();

            return state == ConnectionState.Closed; // connection should close when query is done executing
        }

        public static bool Test_2(DbSqlContext ctx)
        {
            // Disconnnect Test With Execute Query
            ctx.ExecuteQuery<int>("Select Top 1 1").First();

            var state = ctx.GetConnectionState();

            return state == ConnectionState.Closed; // connection should close when query is done executing
        }

        public static bool Test_3(DbSqlContext ctx)
        {
            // Add one record, no foreign keys
            // make sure it has an id
            var policy = _addPolicy(ctx);

            return policy.Id != 0;
        }

        public static bool Test_4(DbSqlContext ctx)
        {
            // Test find method
            var policy = _addPolicy(ctx);

            var foundEntity = ctx.Find<Policy>(policy.Id);

            return foundEntity != null;
        }

        public static bool Test_5(DbSqlContext ctx)
        {
            // Test first or default method
            var policy = _addPolicy(ctx);

            var foundEntity = ctx.From<Policy>().FirstOrDefault(w => w.Id == policy.Id);

            return foundEntity != null;
        }

        public static bool Test_6(DbSqlContext ctx)
        {
            // Test where with first or default method
            var policy = _addPolicy(ctx);

            var foundEntity = ctx.From<Policy>().Where(w => w.Id == policy.Id).FirstOrDefault();

            return foundEntity != null;
        }

        public static bool Test_7(DbSqlContext ctx)
        {
            // Delete one record, no foreign keys
            var policy = _addPolicy(ctx);

            ctx.Delete(policy);

            var foundEntity = ctx.Find<Policy>(policy.Id);

            return foundEntity == null;
        }

        public static bool Test_8(DbSqlContext ctx)
        {
            // Test Disconnect with expression query
            var policy = _addPolicy(ctx);

            var foundEntity = ctx.From<Policy>().Where(w => w.Id == policy.Id).FirstOrDefault();

            var state = ctx.GetConnectionState();

            return state == ConnectionState.Closed;
        }

        public static bool Test_9(DbSqlContext ctx)
        {
            // Test contains in expression query
            var policies = _addPolicies(ctx).Select(w => w.Id).ToList();

            var foundEntities = ctx.From<Policy>().ToList(w => policies.Contains(w.Id));

            return policies.Count == foundEntities.Count;
        }

        public static bool Test_10(DbSqlContext ctx)
        {
            // Test inner join
            var id = ctx.From<PolicyInfo>().Select(w => w.Id).Max();

            var policy = ctx.From<Policy>()
                .InnerJoin(
                    ctx.From<PolicyInfo>(),
                    p => p.PolicyInfoId,
                    pi => pi.Id,
                    (p, pi) => p).FirstOrDefault(w => w.Id == id);

            return policy.Id == id;
        }

        public static bool Test_11(DbSqlContext ctx)
        {
            // Test inner join
            var policies = _addPolicyWithPolicyInfo(ctx);

            var policy = ctx.From<Policy>()
                .LeftJoin(
                    ctx.From<PolicyInfo>(),
                    p => p.PolicyInfoId,
                    pi => pi.Id,
                    (p, pi) => p).FirstOrDefault(w => w.Id == policies.Key.Id);

            return policy.Id == policies.Key.Id;
        }

        public static bool Test_12(DbSqlContext ctx)
        {
            try
            {
                // Test connection opening and closing
                for (var i = 0; i < 100; i++)
                {
                    var item = ctx.From<Policy>().FirstOrDefault(w => w.Id == 1);

                    if (item != null)
                    {
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_13(DbSqlContext ctx)
        {
            // Disconnnect Test With Execute Script
            ctx.ExecuteScript<int>(new CustomScript1()).First();

            var state = ctx.GetConnectionState();

            return state == ConnectionState.Closed;
            // connection should close when query is done executing
        }

        public static bool Test_14(DbSqlContext ctx)
        {
            // Disconnnect Test With Execute Script
            ctx.ExecuteScript(new CustomScript2());

            var state = ctx.GetConnectionState();

            return state == ConnectionState.Closed;
            // connection should close when query is done executing
        }

        public static bool Test_15(DbSqlContext ctx)
        {
            // saving one-one foreign key
            var phoneNumber = new PhoneNumber
            {
                Phone = "555-555-5555",
                PhoneType = new PhoneType
                {
                    Type = "Cell"
                }
            };

            ctx.SaveChanges(phoneNumber);
            return (phoneNumber.ID != 0 && phoneNumber.PhoneType.ID != 0 &&
                    phoneNumber.PhoneTypeID == phoneNumber.PhoneType.ID);
        }

        public static bool Test_16(DbSqlContext ctx)
        {
            try
            {
                // make sure we can update with a timestamp
                var history = new History
                {
                    Description = "Winning",
                    ComputerId = 2
                };

                ctx.SaveChanges(history);

                history.Description = "Changed";

                ctx.SaveChanges(history);

                return history.Description == "Changed";
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_17(DbSqlContext ctx)
        {
            // insert complex object
            var contact = _addContact(ctx);

            // test to make sure all data was inserted correctly
            // keep all variables so we can tell what fails if any
            var test1 = contact.Appointments.All(w => w.ContactID == contact.ContactID);
            var test2 = contact.Appointments.All(w => w.ID != Guid.Empty) && contact.ContactID != 0;
            var test3 = contact.CreatedBy.ID != 0 && contact.CreatedByUserID == contact.CreatedBy.ID;
            var test4 = contact.EditedBy.ID != 0 && contact.EditedByUserID == contact.EditedBy.ID;
            var test5 = contact.Names.All(w => w.ContactID == contact.ContactID);
            var test6 = contact.Names.All(w => w.ID != 0);
            var test7 = contact.Appointments.All(w => w.Address.All(x => x.AppointmentID == w.ID));
            var test8 = contact.Appointments.All(w => w.Address.All(x => x.ID != 0));
            var test9 = contact.Appointments.All(w => w.Address.All(x => x.ZipCode.All(z => z.AddressID == x.ID)));
            var test10 = contact.Appointments.All(w => w.Address.All(x => x.ZipCode.All(z => z.ID != 0)));
            var test11 = contact.Appointments.All(w => w.Address.All(x => x.State.ID == x.StateID));
            var test12 = contact.Appointments.All(w => w.Address.All(x => x.State.ID != 0));
            var test13 = contact.Number.ID == contact.PhoneID;
            var test14 = contact.Number.PhoneType.ID == contact.Number.PhoneTypeID && contact.Number.PhoneTypeID != 0;

            return (test1 && test2 && test3 && test4 && test5 && test6 && test7 && test8 && test9 && test10 &&
                          test11 && test12 && test13 && test14);
        }

        public static bool Test_18(DbSqlContext ctx)
        {
            // delete complex object
            var contact = _addContact(ctx);

            ctx.Delete(contact);

            // test to make sure all data was inserted correctly
            // keep all variables so we can tell what fails if any
            var test1 = ctx.From<Appointment>().ToList(w => w.ContactID == contact.ContactID).Count == 0;
            var test2 = ctx.Find<User>(contact.EditedByUserID) == null;
            var test3 = ctx.Find<User>(contact.CreatedByUserID) == null;
            var test4 = ctx.From<Name>().ToList(w => w.ContactID == contact.ContactID).Count == 0;
            bool test5 = true;

            foreach (var appointment in contact.Appointments)
            {
                var oldApt = contact.Appointments.First(w => w.ID == appointment.ID);

                foreach (var address in oldApt.Address)
                {
                    test5 = ctx.Find<Address>(address.ID) == null;

                    if (!test5) break;

                    test5 = ctx.Find<StateCode>(address.StateID) == null;

                    if (!test5) break;

                    foreach (var zip in address.ZipCode)
                    {
                        test5 = ctx.Find<Zip>(zip.ID) == null;

                        if (!test5) break;
                    }
                }
            }

            var test6 = ctx.Find<PhoneNumber>(contact.Number.ID) == null;
            var test7 = ctx.Find<PhoneType>(contact.Number.PhoneTypeID) == null;

            return (test1 && test2 && test3 && test4 && test5 && test6 && test7);
        }

        public static bool Test_19(DbSqlContext ctx)
        {
            // throw exception on readonly table save
            try
            {
                var person = new Person
                {
                    City = "Anywhere",
                    FirstName = "James",
                    LastName = "Demeuse",
                    State = "MN",
                    Zip = "12345",
                    StreetAddress = "123 2nd St"
                };

                ctx.SaveChanges(person);

                return false;
            }
            catch (SqlSaveException)
            {
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_20(DbSqlContext ctx)
        {
            // check to see if we can read from a readonly table
            var person = ctx.Find<Person>(2);

            return (person != null);
        }

        public static bool Test_21(DbSqlContext ctx)
        {
            // test dispose to make sure our context disposes correctly
            var person = ctx.Find<Person>(2);

            ctx.Dispose();

            // DatabaseSchematic
            // Protected Properties: Tables
            // Private Properties: _tableScriptMappings

            // Database
            // Protected Properties:  ConnectionString, Reader, Configuration
            // Protected Fields:  OnSqlGeneration
            // Private Properties: _connection, _command, _configurationCheckPerformed (= false)

            // DatabaseQuery
            // Private Properties:  _schematicManager

            // DatabaseModifiable
            // Protected Fields:  OnBeforeSave, OnAfterSave, OnSaving, OnConcurrencyViolation

            var Tables = _getPropertyValue(ctx, typeof(DatabaseSchematic), "DbTableFactory") == null;
            var _tableScriptMappings = _getPropertyValue(ctx, typeof(DatabaseSchematic), "_tableScriptMappings") == null;

            var ConnectionString = _getPropertyValue(ctx, "ConnectionString") == null;
            var Reader = _getPropertyValue(ctx, "Reader") == null;
            var Configuration = _getPropertyValue(ctx, "Configuration") == null;
            var OnSqlGeneration = _getEventValue(ctx, "OnSqlGeneration") == null;
            var _connection = _getPropertyValue(ctx, typeof(Database), "_connection") == null;
            var _command = _getPropertyValue(ctx, typeof(Database), "_command") == null;
            var _configurationCheckPerformed = (bool)_getPropertyValue(ctx, typeof(Database), "_configurationCheckPerformed") == false;

            var _schematicManager = _getPropertyValue(ctx, typeof(DatabaseQuery), "SchematicFactory") == null;

            var OnBeforeSave = _getEventValue(ctx, "OnBeforeSave") == null;
            var OnAfterSave = _getEventValue(ctx, "OnAfterSave") == null;
            var OnSaving = _getEventValue(ctx, "OnSaving") == null;
            var OnConcurrencyViolation = _getEventValue(ctx, "OnConcurrencyViolation") == null;

            return Tables && _tableScriptMappings && ConnectionString && Reader && Configuration && OnSqlGeneration &&
                   _connection && _command && _configurationCheckPerformed && _schematicManager && OnBeforeSave &&
                   OnAfterSave && OnSaving && OnConcurrencyViolation;
        }

        public static bool Test_22(DbSqlContext ctx)
        {
            try
            {
                var names = new List<string> { "James", "Megan" };
                var test = ctx.From<Contact>().Where(w => names.Contains(w.FirstName)).Count(w => w.ContactID == 10);
            }
            catch (Exception)
            {

                throw;
            }

            return true;
        }

        public static bool Test_23(DbSqlContext ctx)
        {
            try
            {
                // Testing renaming of FK Id and nullable FK
                var pizza = _addPizza(ctx);

                var test1 = pizza.Id != 0 && pizza.ToppingRenameId == pizza.Topping.Id && pizza.Topping.Id != 0;
                var test2 = pizza.Crust.Id == pizza.CrustId && pizza.CrustId != 0;
                var test3 = pizza.Crust.Topping.Id == pizza.Crust.ToppingId && pizza.Crust.ToppingId != 0;

                return (test1 && test2 && test3);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_24(DbSqlContext ctx)
        {
            try
            {
                // remove pizza with renamed FK id
                var pizza = _addPizza(ctx);

                ctx.Delete(pizza);

                var test1 = ctx.Find<Pizza>(pizza.Id) == null;
                var test2 = ctx.Find<Topping>(pizza.ToppingRenameId) == null;
                var test3 = ctx.Find<Crust>(pizza.CrustId) == null;
                var test4 = ctx.Find<Topping>(pizza.Crust.ToppingId) == null;

                return (test1 && test2 && test3 && test4);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_25(DbSqlContext ctx)
        {
            try
            {
                // Save with nullable foreign key
                var contact = new Contact
                {
                    CreatedBy = new User
                    {
                        Name = "James Demeuse"
                    },
                    EditedBy = new User
                    {
                        Name = "Different User"
                    },
                    FirstName = "Test",
                    LastName = "User",
                    Names = new List<Name>
                    {
                        new Name
                        {
                            Value = "Win!"
                        },
                        new Name
                        {
                            Value = "FTW!"
                        }
                    },
                    Appointments = new List<Appointment>
                    {
                        new Appointment
                        {
                            Description = "Appointment 1",
                            IsScheduled = false,
                            Address = new List<Address>
                            {
                                new Address
                                {
                                    Addy = "1234 First Ave South",
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
                        }
                    }
                };

                ctx.SaveChanges(contact);

                var that = ctx.Find<Contact>(contact.ContactID);

                return (that != null && that.Number == null);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_26(DbSqlContext ctx)
        {
            try
            {
                // Save with two PK's
                var linking = new Linking
                {
                    Description = "Test",
                    PolicyId = 10,
                    PolicyInfoId = 1
                };

                ctx.SaveChanges(linking);

                var that = ctx.Find<Linking>(10, 1);

                return (that != null);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_27(DbSqlContext ctx)
        {
            try
            {
                // Save with two PK's
                var linking = new Linking
                {
                    Description = "Test",
                    PolicyId = 10,
                    PolicyInfoId = 1
                };

                ctx.SaveChanges(linking);

                var that = ctx.Find<Linking>(10, 1);

                return (that != null);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_28(DbSqlContext ctx)
        {
            // saving in a lookup table
            var computer = new Computer
            {
                Name = "James PC",
                IsCustom = true,
                Processor = new Processor
                {
                    Name = "intel",
                    CoreType = CoreType.AMD,
                    Cores = 4,
                    Speed = 4
                },
                History = new List<History>
                {
                    new History
                    {
                        Description = "WINNING"
                    }
                }
            };

            ctx.SaveChanges(computer);

            var that = ctx.Find<Computer>(computer.Id);

            return (that != null && ctx.Find<Processor>(computer.ProcessorId) != null &&
                          (that.History == null || !that.History.Any(w => w.ComputerId == computer.Id)));
        }

        public static bool Test_29(DbSqlContext ctx)
        {
            // make sure the Lookup Table is directly editable,
            // IF FAIL!!!!!
            // Make sure the ComputerId exists first
            var history = new History
            {
                Description = "Winning",
                ComputerId = 2
            };

            ctx.SaveChanges(history);

            return (history.Id != 0);
        }

        public static bool Test_30(DbSqlContext ctx)
        {
            // making sure nullable types work
            var computer = new Computer
            {
                Name = "James PC",
                IsCustom = true,
                Processor = new Processor
                {
                    Name = "intel",
                    Cores = 4
                },
                History = new List<History>
                {
                    new History
                    {
                        Description = "WINNING"
                    }
                }
            };

            ctx.SaveChanges(computer);

            var that = ctx.Find<Computer>(computer.Id);

            ctx.Delete(computer);

            return (!that.Processor.Speed.HasValue && !that.Processor.CoreType.HasValue);
        }

        public static bool Test_31(DbSqlContext ctx)
        {
            try
            {
                // Should get errors when we try to set a value in
                // a timestamp column with an insert
                var history = new History
                {
                    Description = "Winning",
                    ComputerId = 2,
                    CreateDate = new byte[] { 0, 0, 0, 0, 68, 30 }
                };

                ctx.SaveChanges(history);

                return false;
            }
            catch (SqlSaveException)
            {
                return true;
            }
        }

        public static bool Test_32(DbSqlContext ctx)
        {
            // Make sure the any function works
            var policy = _addPolicy(ctx);

            var result = (ctx.From<Policy>().Any(w => w.Id == policy.Id));

            // cleanup
            ctx.Delete(policy);

            return result;
        }

        public static bool Test_33(DbSqlContext ctx)
        {
            // Make sure the any function works
            var policy = _addPolicy(ctx);

            var result = (ctx.From<Policy>().Any());

            // cleanup
            ctx.Delete(policy);

            return result;
        }

        public static bool Test_34(DbSqlContext ctx)
        {
            // Make sure the any function works
            var policy = _addPolicy(ctx);

            var result = (ctx.From<Policy>().Count() != 0);

            // cleanup
            ctx.Delete(policy);

            return result;
        }

        public static bool Test_35(DbSqlContext ctx)
        {
            try
            {
                // make sure first is working, should fail when no records found
                ctx.From<Policy>().First(w => w.Id == -1);

                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static bool Test_36(DbSqlContext ctx)
        {
            try
            {
                // make sure first finds records
                var policy = _addPolicy(ctx);

                var result = (ctx.From<Policy>().First(w => w.Id == policy.Id) != null);

                // cleanup
                ctx.Delete(policy);

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_37(DbSqlContext ctx)
        {
            try
            {
                // make sure first is working, should fail when no records found
                return (ctx.From<Policy>().FirstOrDefault(w => w.Id == -1) == null);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_38(DbSqlContext ctx)
        {
            try
            {
                // make sure first finds records
                var policy = _addPolicy(ctx);

                var result = (ctx.From<Policy>().FirstOrDefault(w => w.Id == policy.Id) != null);

                // cleanup
                ctx.Delete(policy);

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_39(DbSqlContext ctx)
        {
            // max should return the max id
            var max = ctx.From<Policy>().Select(w => w.Id).Max();

            return (max != 0);
        }

        public static bool Test_40(DbSqlContext ctx)
        {
            // min should return the min id
            var min = ctx.From<Policy>().Select(w => w.Id).Min();

            return (min != 0);
        }

        public static bool Test_41(DbSqlContext ctx)
        {
            // make sure order by is working
            var allItems = ctx.From<Policy>().OrderByDescending(w => w.Id).Take(2).ToList();

            return (allItems[0].Id > allItems[1].Id);
        }

        public static bool Test_42(DbSqlContext ctx)
        {
            // make sure order by is working
            var allItems = ctx.From<Policy>().OrderBy(w => w.Id).Take(2).ToList();

            return (allItems[1].Id > allItems[0].Id);
        }

        public static bool Test_43(DbSqlContext ctx)
        {
            //for (int i = 0; i < 1000; i++)
            //{
            //    _addPolicyWithPolicyInfo();
            //}

            // make sure then by is working
            var allItems = ctx.From<Policy>().OrderByDescending(w => w.Id).ThenBy(w => w.PolicyDate).Take(500).ToList();

            return (allItems[0].Id > allItems[499].Id && allItems[0].PolicyDate > allItems[499].PolicyDate);
        }

        public static bool Test_44(DbSqlContext ctx)
        {
            // make sure select is working properly
            var allItems = ctx.From<Policy>().Where(w => w.Id == 1).Select(w => w.InsuredName).First();

            return (!string.IsNullOrWhiteSpace(allItems));
        }

        public static bool Test_45(DbSqlContext ctx)
        {
            // individual method for take, even though it is test above
            var allItems = ctx.From<Policy>().Take(10).Select(w => w.InsuredName).ToList();

            return (allItems.Count == 10);
        }

        public static bool Test_46(DbSqlContext ctx)
        {
            // individual method for take, even though it is test above
            var allItems = ctx.From<Policy>().Select(w => w.PolicyInfoId).Distinct().ToList();

            return (allItems.Count > 0);
        }

        public static bool Test_47(DbSqlContext ctx)
        {
            // Should be able to select child of a child (not a list) and perform query from it
            var allItems = ctx.From<Contact>().Where(w => w.Number.PhoneType.Type == "Cell").ToList();

            return (allItems.Select(w => w.Number.PhoneType.Type).All(w => w == "Cell"));
        }

        public static bool Test_48(DbSqlContext ctx)
        {
            // Test Pseudo Keys
            //var artist = new Artist
            //{
            //    ActiveDate = DateTime.Now,
            //    Albums = new List<Album>
            //    {
            //        new Album
            //        {
            //            Name = "COOLE",
            //            TimesDownloaded = 1000
            //        },
            //        new Album
            //        {
            //            Name = "EP",
            //            TimesDownloaded = 165
            //        }
            //    },
            //    Agent = new Agent
            //    {
            //        Name = "James Demeuse"
            //    },
            //    FirstName = "Some",
            //    LastName = "Singer",
            //    Genre = "Country"
            //};

            //5,6

            //ctx.SaveChanges(artist);

            var artist1 = ctx.Find<Artist>(5);
            var artist2 = ctx.Find<Artist>(6);

            return (artist1 != null && artist2 != null && artist1.Albums.All(w => w.ArtistId == artist1.Id) &&
                    artist2.Albums.All(w => w.ArtistId == artist2.Id) && artist1.Agent.Id == artist1.AgentId &&
                    artist2.Agent.Id == artist2.AgentId);
        }

        public static bool Test_49(DbSqlContext ctx)
        {
            // try insert, happens when a table has only PK's
            var newRefId = ctx.From<TryInsert>().Select(w => w.RefId).Max() + 1;
            var newSomeId = ctx.From<TryInsert>().Select(w => w.SomeId).Max() + 1;

            var item = new TryInsert
            {
                RefId = newRefId,
                SomeId = newSomeId
            };

            var anyTryOne = ctx.From<TryInsert>().Any(w => w.RefId == newRefId && w.SomeId == newSomeId);

            ctx.SaveChanges(item);

            var anyTryTwo = ctx.From<TryInsert>().Any(w => w.RefId == newRefId && w.SomeId == newSomeId);

            return (!anyTryOne && anyTryTwo);
        }

        public static bool Test_50(DbSqlContext ctx)
        {
            // try insert, nothing should be inserted
            var newRefId = ctx.From<TryInsert>().Select(w => w.RefId).Max();
            var newSomeId = ctx.From<TryInsert>().Select(w => w.SomeId).Max();

            var item = new TryInsert
            {
                RefId = newRefId,
                SomeId = newSomeId
            };

            ctx.SaveChanges(item);

            return (ctx.From<TryInsert>().Any(w => w.RefId == newRefId && w.SomeId == newSomeId));
        }

        public static bool Test_51(DbSqlContext ctx)
        {
            // try insert update
            var newId = ctx.From<TryInsertUpdateWithGeneration>().Select(w => w.Id).Max() + 1;

            var item = new TryInsertUpdateWithGeneration
            {
                Id = newId,
                Name = "Testing"
            };

            ctx.SaveChanges(item);

            return (ctx.From<TryInsertUpdateWithGeneration>().Any(w => w.Id == newId) &&
                item.SequenceNumber != 0 &&
                item.OtherNumber != 0);
        }

        public static bool Test_52(DbSqlContext ctx)
        {
            // try insert update, cannot update identity column
            var newId = ctx.From<TryInsertUpdateWithGeneration>().Select(w => w.Id).Max();

            var item = ctx.Find<TryInsertUpdateWithGeneration>(newId);

            item.OtherNumber++;

            try
            {
                ctx.SaveChanges(item);
                return false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public static bool Test_53(DbSqlContext ctx)
        {
            // try insert update should fail if the PK(s) are 0
            var item = new TryInsertUpdateWithGeneration
            {
                Id = 0,
                Name = "Testing",
                OtherNumber = 10
            };

            try
            {
                ctx.SaveChanges(item);
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static bool Test_54(DbSqlContext ctx)
        {
            // Update should be performed even if the id doesnt exist
            var item = new TestDefaultInsert
            {
                Name = "Test",
                Id = 100000
            };

            try
            {
                ctx.SaveChanges(item);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_55(DbSqlContext ctx)
        {
            // should use the db default and load it back into the model
            var item = new TestDefaultInsert
            {
                Name = "Test",
            };

            ctx.SaveChanges(item);

            return (item.Uid != Guid.Empty);
        }

        public static bool Test_56(DbSqlContext ctx)
        {
            // test update
            var contact = _addContact(ctx);

            contact.FirstName = "Joe Blow";

            ctx.SaveChanges(contact);

            contact = ctx.Find<Contact>(contact.ContactID);

            return (contact.FirstName == "Joe Blow");
        }

        public static bool Test_57(DbSqlContext ctx)
        {
            var testOne = true;
            var testTwo = true;
            var testThree = true;

            // test insert
            var item = new TestUpdateWithKeyDbGenerationOptionNone
            {
                Name = "Name",
                Id = 0
            };

            try
            {
                ctx.SaveChanges(item);
            }
            catch (Exception)
            {
                testOne = false;
            }

            item.Id = ctx.From<TestUpdateWithKeyDbGenerationOptionNone>().Select(w => w.Id).Max() + 1;

            ctx.SaveChanges(item);

            item = ctx.Find<TestUpdateWithKeyDbGenerationOptionNone>(item.Id);

            testTwo = item != null;

            item.Name = "NEW NAME!";

            ctx.SaveChanges(item);

            item = ctx.Find<TestUpdateWithKeyDbGenerationOptionNone>(item.Id);

            testThree = item.Name == "NEW NAME!";

            return (testOne && testTwo && testThree);
        }

        public static bool Test_58(DbSqlContext ctx)
        {
            var item = new TestKeyWithDbDefaultGeneration
            {
                Name = "Name"
            };

            try
            {
                ctx.SaveChanges(item);
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static bool Test_59(DbSqlContext ctx)
        {
            // schema changes
            var item = new SchemaChangeOne_dbo
            {
                Name = "Name"
            };

            ctx.SaveChanges(item);

            item = ctx.Find<SchemaChangeOne_dbo>(item.Id);

            return (item != null);
        }

        public static bool Test_60(DbSqlContext ctx)
        {
            // schema changes for normal insert that generates a pk
            var item = new SchemaChangeOne_ts
            {
                Name = "Name"
            };

            ctx.SaveChanges(item);

            item = ctx.Find<SchemaChangeOne_ts>(item.Id);

            return (item != null);
        }

        public static bool Test_61(DbSqlContext ctx)
        {
            // schema changes for try insert update
            var newId = ctx.From<TestUpdateWithKeyDbGenerationOptionNone_ts>().Select(w => w.Id).Max() + 1;

            // test insert
            var item = new TestUpdateWithKeyDbGenerationOptionNone_ts
            {
                Name = "Name",
                Id = newId
            };

            ctx.SaveChanges(item);

            item = ctx.Find<TestUpdateWithKeyDbGenerationOptionNone_ts>(item.Id);

            return (item != null);
        }

        public static bool Test_62(DbSqlContext ctx)
        {
            // test updates to tables in different schema
            var item = new TestUpdateNewSchema
            {
                Name = "Name"
            };

            ctx.SaveChanges(item);

            item = ctx.Find<TestUpdateNewSchema>(item.Id);

            item.Name = "NEW NAME!";

            ctx.SaveChanges(item);

            item = ctx.Find<TestUpdateNewSchema>(item.Id);

            return (item.Name == "NEW NAME!");
        }

        public static bool Test_63(DbSqlContext ctx)
        {
            try
            {
                // previous issue
                var count = ctx.From<Contact>().Where(w => w.FirstName.Contains("Test")).Count(w => w.LastName.Contains("Use"));

                return count > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_64(DbSqlContext ctx)
        {
            // make sure count comes back
            var test = ctx.From<Contact>().Where(w => w.FirstName.Contains("Te")).Count(w => w.ContactID == 10);

            return (test != 0);
        }

        public static bool Test_65(DbSqlContext ctx)
        {
            // save should fail because of foreign keys
            try
            {
                var contact = new Contact
                {
                    FirstName = "Test",
                    LastName = "User"
                };

                ctx.SaveChanges(contact);

                return false;
            }
            catch (SqlSaveException ex)
            {
                return true;
            }
        }

        public static bool Test_66(DbSqlContext ctx)
        {
            try
            {
                // performs a try insert update
                var max1 = ctx.From<Linking>().Select(w => w.PolicyId).Max() + 1;
                var max2 = ctx.From<Linking>().Select(w => w.PolicyInfoId).Max() + 1;

                // make sure inserting and updating is working for the try insert update
                var linking = new Linking
                {
                    Description = "Test",
                    PolicyId = max1,
                    PolicyInfoId = max2
                };

                ctx.SaveChanges(linking);

                var that = ctx.Find<Linking>(max1, max2);

                that.Description = "Changed!";

                ctx.SaveChanges(that);

                that = ctx.Find<Linking>(max1, max2);

                var changed = that.Description;

                return (that != null && changed == "Changed!");
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_67(DbSqlContext ctx)
        {
            // make sure we cannot update identity column when EST is on
            var id = ctx.From<TryInsertUpdateWithGeneration>().Select(w => w.Id).Max();

            var item = ctx.Find<TryInsertUpdateWithGeneration>(id);

            item.OtherNumber = 20;

            try
            {
                ctx.SaveChanges(item);
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static bool Test_68(DbSqlContext ctx)
        {
            // save timestamp test
            var history = new History
            {
                Description = "Winning",
                ComputerId = 2
            };

            ctx.SaveChanges(history);

            return (history.CreateDate != null);
        }

        public static bool Test_69(DbSqlContext ctx)
        {
            // test insert after parent is saved already
            var contact = new Contact
            {
                CreatedBy = new User
                {
                    Name = "James Demeuse"
                },
                EditedBy = new User
                {
                    Name = "Different User"
                },
                
            };

            ctx.SaveChanges(contact);

            contact.Number = new PhoneNumber
            {
                Phone = "555-555-5555",
                PhoneType = new PhoneType
                {
                    Type = "Cell"
                }
            };

            ctx.SaveChanges(contact);

            return contact.PhoneID.HasValue;
        }

        #region helpers
        protected static Policy _addPolicy(DbSqlContext ctx)
        {
            var policy = new Policy
            {
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

            return policy;
        }

        protected static Pizza _addPizza(DbSqlContext ctx)
        {
            var pizza = new Pizza
            {
                CookTime = 35,
                Name = "Deep Dish",
                Topping = new Topping
                {
                    Cost = .20m,
                    Name = "Onion"
                },
                Crust = new Crust
                {
                    Name = "Stuffed",
                    Topping = new Topping
                    {
                        Cost = 1.35m,
                        Name = "Butter, Cheese"
                    }
                },
                DeliveryMan = new DeliveryMan
                {
                    AverageDeliveryTime = 15,
                    FirstName = "James",
                    LastName = "Demeuse",
                }
            };

            ctx.SaveChanges(pizza);

            return pizza;
        }

        protected static Contact _addContact(DbSqlContext ctx)
        {
            var contact = new Contact
            {
                CreatedBy = new User
                {
                    Name = "James Demeuse"
                },
                EditedBy = new User
                {
                    Name = "Different User"
                },
                FirstName = "Test",
                LastName = "User",
                Names = new List<Name>
                {
                    new Name
                    {
                        Value = "Win!"
                    },
                    new Name
                    {
                        Value = "FTW!"
                    }
                },
                Number = new PhoneNumber
                {
                    Phone = "555-555-5555",
                    PhoneType = new PhoneType
                    {
                        Type = "Cell"
                    }
                },
                Appointments = new List<Appointment>
                {
                    new Appointment
                    {
                        Description = "Appointment 1",
                        IsScheduled = false,
                        Address = new List<Address>
                        {
                            new Address
                            {
                                Addy = "1234 First Ave South",
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
                    }
                }
            };

            ctx.SaveChanges(contact);

            return contact;
        }

        private static void _deleteAllData(DbSqlContext ctx)
        {
            const string sql = @"
    Delete From [User];
    Delete From Name;
    Delete From ZipCode;
    Delete From [Address];
    Delete From StateCode;
    Delete From Appointments;
    Delete From Contacts;
    Delete From PhoneType;
    Delete From PhoneNumbers;
";
        }

        private static KeyValuePair<Policy, PolicyInfo> _addPolicyWithPolicyInfo(DbSqlContext ctx)
        {
            var policyInfo = new PolicyInfo
            {
                Description = "info",
                FirstName = "james",
                LastName = "demeuse",
                Stamp = Guid.NewGuid()
            };

            ctx.SaveChanges(policyInfo);

            var policy = new Policy
            {
                County = "Hennepin",
                CreatedBy = "James Demeuse",
                CreatedDate = DateTime.Now,
                FeeOwnerName = "Test",
                FileNumber = 100,
                InsuredName = "James Demeuse",
                PolicyAmount = 100,
                PolicyDate = DateTime.Now,
                PolicyInfoId = policyInfo.Id,
                PolicyNumber = "001-8A",
                StateID = 7,
                UpdatedBy = "Me",
                UpdatedDate = DateTime.Now,
            };

            ctx.SaveChanges(policy);

            return new KeyValuePair<Policy, PolicyInfo>(policy, policyInfo);
        }

        private static List<Policy> _addPolicies(DbSqlContext ctx)
        {
            var result = new List<Policy>();

            for (var i = 0; i < 100; i++)
            {
                result.Add(_addPolicy(ctx));
            }

            return result;
        }

        private static object _getPropertyValue(object entity, string propertyName)
        {
            var property = entity.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

            return property.GetValue(entity);
        }

        private static object _getEventValue(object entity, string propertyName)
        {
            var e = entity.GetType().GetEvent(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

            return e.GetRaiseMethod();
        }


        private static object _getFieldValue(object entity, string propertyName)
        {
            var field = entity.GetType().GetField(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

            return field.GetValue(entity);
        }

        private static object _getPropertyValue(object entity, Type type, string propertyName)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

            return property.GetValue(entity);
        }
        #endregion
    }
}

namespace OR_M_Data_Entities.Tests.Testing.BaseESTOff
{
    using Tables.EntityStateTrackableOff;

    public class DefaultTestsESTOff
    {
        public static bool Test_0(DbSqlContext ctx)
        {
            throw new NotImplementedException();
        }

        public static bool Test_1(DbSqlContext ctx)
        {
            // Disconnnect Test With Execute Query
            ctx.ExecuteQuery("Select Top 1 1");

            var state = ctx.GetConnectionState();

            return state == ConnectionState.Closed; // connection should close when query is done executing
        }

        public static bool Test_2(DbSqlContext ctx)
        {
            // Disconnnect Test With Execute Query
            ctx.ExecuteQuery<int>("Select Top 1 1").First();

            var state = ctx.GetConnectionState();

            return state == ConnectionState.Closed; // connection should close when query is done executing
        }

        public static bool Test_3(DbSqlContext ctx)
        {
            // Add one record, no foreign keys
            // make sure it has an id
            var policy = _addPolicy(ctx);

            return policy.Id != 0;
        }

        public static bool Test_4(DbSqlContext ctx)
        {
            // Test find method
            var policy = _addPolicy(ctx);

            var foundEntity = ctx.Find<Policy>(policy.Id);

            return foundEntity != null;
        }

        public static bool Test_5(DbSqlContext ctx)
        {
            // Test first or default method
            var policy = _addPolicy(ctx);

            var foundEntity = ctx.From<Policy>().FirstOrDefault(w => w.Id == policy.Id);

            return foundEntity != null;
        }

        public static bool Test_6(DbSqlContext ctx)
        {
            // Test where with first or default method
            var policy = _addPolicy(ctx);

            var foundEntity = ctx.From<Policy>().Where(w => w.Id == policy.Id).FirstOrDefault();

            return foundEntity != null;
        }

        public static bool Test_7(DbSqlContext ctx)
        {
            // Delete one record, no foreign keys
            var policy = _addPolicy(ctx);

            ctx.Delete(policy);

            var foundEntity = ctx.Find<Policy>(policy.Id);

            return foundEntity == null;
        }

        public static bool Test_8(DbSqlContext ctx)
        {
            // Test Disconnect with expression query
            var policy = _addPolicy(ctx);

            var foundEntity = ctx.From<Policy>().Where(w => w.Id == policy.Id).FirstOrDefault();

            var state = ctx.GetConnectionState();

            return state == ConnectionState.Closed;
        }

        public static bool Test_9(DbSqlContext ctx)
        {
            // Test contains in expression query
            var policies = _addPolicies(ctx).Select(w => w.Id).ToList();

            var foundEntities = ctx.From<Policy>().ToList(w => policies.Contains(w.Id));

            return policies.Count == foundEntities.Count;
        }

        public static bool Test_10(DbSqlContext ctx)
        {
            // Test inner join
            var id = ctx.From<PolicyInfo>().Select(w => w.Id).Max();

            var policy = ctx.From<Policy>()
                .InnerJoin(
                    ctx.From<PolicyInfo>(),
                    p => p.PolicyInfoId,
                    pi => pi.Id,
                    (p, pi) => p).FirstOrDefault(w => w.Id == id);

            return policy.Id == id;
        }

        public static bool Test_11(DbSqlContext ctx)
        {
            // Test inner join
            var policies = _addPolicyWithPolicyInfo(ctx);

            var policy = ctx.From<Policy>()
                .LeftJoin(
                    ctx.From<PolicyInfo>(),
                    p => p.PolicyInfoId,
                    pi => pi.Id,
                    (p, pi) => p).FirstOrDefault(w => w.Id == policies.Key.Id);

            return policy.Id == policies.Key.Id;
        }

        public static bool Test_12(DbSqlContext ctx)
        {
            try
            {
                // Test connection opening and closing
                for (var i = 0; i < 100; i++)
                {
                    var item = ctx.From<Policy>().FirstOrDefault(w => w.Id == 1);

                    if (item != null)
                    {
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_13(DbSqlContext ctx)
        {
            // Disconnnect Test With Execute Script
            ctx.ExecuteScript<int>(new CustomScript1()).First();

            var state = ctx.GetConnectionState();

            return state == ConnectionState.Closed;
            // connection should close when query is done executing
        }

        public static bool Test_14(DbSqlContext ctx)
        {
            // Disconnnect Test With Execute Script
            ctx.ExecuteScript(new CustomScript2());

            var state = ctx.GetConnectionState();

            return state == ConnectionState.Closed;
            // connection should close when query is done executing
        }

        public static bool Test_15(DbSqlContext ctx)
        {
            // saving one-one foreign key
            var phoneNumber = new PhoneNumber
            {
                Phone = "555-555-5555",
                PhoneType = new PhoneType
                {
                    Type = "Cell"
                }
            };

            ctx.SaveChanges(phoneNumber);
            return (phoneNumber.ID != 0 && phoneNumber.PhoneType.ID != 0 &&
                    phoneNumber.PhoneTypeID == phoneNumber.PhoneType.ID);
        }

        public static bool Test_16(DbSqlContext ctx)
        {
            try
            {
                // make sure we can update with a timestamp
                var history = new History
                {
                    Description = "Winning",
                    ComputerId = 2
                };

                ctx.SaveChanges(history);

                history.Description = "Changed";

                ctx.SaveChanges(history);

                return history.Description == "Changed";
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_17(DbSqlContext ctx)
        {
            // insert complex object
            var contact = _addContact(ctx);

            // test to make sure all data was inserted correctly
            // keep all variables so we can tell what fails if any
            var test1 = contact.Appointments.All(w => w.ContactID == contact.ContactID);
            var test2 = contact.Appointments.All(w => w.ID != Guid.Empty) && contact.ContactID != 0;
            var test3 = contact.CreatedBy.ID != 0 && contact.CreatedByUserID == contact.CreatedBy.ID;
            var test4 = contact.EditedBy.ID != 0 && contact.EditedByUserID == contact.EditedBy.ID;
            var test5 = contact.Names.All(w => w.ContactID == contact.ContactID);
            var test6 = contact.Names.All(w => w.ID != 0);
            var test7 = contact.Appointments.All(w => w.Address.All(x => x.AppointmentID == w.ID));
            var test8 = contact.Appointments.All(w => w.Address.All(x => x.ID != 0));
            var test9 = contact.Appointments.All(w => w.Address.All(x => x.ZipCode.All(z => z.AddressID == x.ID)));
            var test10 = contact.Appointments.All(w => w.Address.All(x => x.ZipCode.All(z => z.ID != 0)));
            var test11 = contact.Appointments.All(w => w.Address.All(x => x.State.ID == x.StateID));
            var test12 = contact.Appointments.All(w => w.Address.All(x => x.State.ID != 0));
            var test13 = contact.Number.ID == contact.PhoneID;
            var test14 = contact.Number.PhoneType.ID == contact.Number.PhoneTypeID && contact.Number.PhoneTypeID != 0;

            return (test1 && test2 && test3 && test4 && test5 && test6 && test7 && test8 && test9 && test10 &&
                          test11 && test12 && test13 && test14);
        }

        public static bool Test_18(DbSqlContext ctx)
        {
            // delete complex object
            var contact = _addContact(ctx);

            ctx.Delete(contact);

            // test to make sure all data was inserted correctly
            // keep all variables so we can tell what fails if any
            var test1 = ctx.From<Appointment>().ToList(w => w.ContactID == contact.ContactID).Count == 0;
            var test2 = ctx.Find<User>(contact.EditedByUserID) == null;
            var test3 = ctx.Find<User>(contact.CreatedByUserID) == null;
            var test4 = ctx.From<Name>().ToList(w => w.ContactID == contact.ContactID).Count == 0;
            bool test5 = true;

            foreach (var appointment in contact.Appointments)
            {
                var oldApt = contact.Appointments.First(w => w.ID == appointment.ID);

                foreach (var address in oldApt.Address)
                {
                    test5 = ctx.Find<Address>(address.ID) == null;

                    if (!test5) break;

                    test5 = ctx.Find<StateCode>(address.StateID) == null;

                    if (!test5) break;

                    foreach (var zip in address.ZipCode)
                    {
                        test5 = ctx.Find<Zip>(zip.ID) == null;

                        if (!test5) break;
                    }
                }
            }

            var test6 = ctx.Find<PhoneNumber>(contact.Number.ID) == null;
            var test7 = ctx.Find<PhoneType>(contact.Number.PhoneTypeID) == null;

            return (test1 && test2 && test3 && test4 && test5 && test6 && test7);
        }

        public static bool Test_19(DbSqlContext ctx)
        {
            // throw exception on readonly table save
            try
            {
                var person = new Person
                {
                    City = "Anywhere",
                    FirstName = "James",
                    LastName = "Demeuse",
                    State = "MN",
                    Zip = "12345",
                    StreetAddress = "123 2nd St"
                };

                ctx.SaveChanges(person);

                return false;
            }
            catch (SqlSaveException)
            {
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_20(DbSqlContext ctx)
        {
            // check to see if we can read from a readonly table
            var person = ctx.Find<Person>(2);

            return (person != null);
        }

        public static bool Test_21(DbSqlContext ctx)
        {
            // test dispose to make sure our context disposes correctly
            var person = ctx.Find<Person>(2);

            ctx.Dispose();

            // DatabaseSchematic
            // Protected Properties: Tables
            // Private Properties: _tableScriptMappings

            // Database
            // Protected Properties:  ConnectionString, Reader, Configuration
            // Protected Fields:  OnSqlGeneration
            // Private Properties: _connection, _command, _configurationCheckPerformed (= false)

            // DatabaseQuery
            // Private Properties:  _schematicManager

            // DatabaseModifiable
            // Protected Fields:  OnBeforeSave, OnAfterSave, OnSaving, OnConcurrencyViolation

            var Tables = _getPropertyValue(ctx, typeof(DatabaseSchematic), "DbTableFactory") == null;
            var _tableScriptMappings = _getPropertyValue(ctx, typeof(DatabaseSchematic), "_tableScriptMappings") == null;

            var ConnectionString = _getPropertyValue(ctx, "ConnectionString") == null;
            var Reader = _getPropertyValue(ctx, "Reader") == null;
            var Configuration = _getPropertyValue(ctx, "Configuration") == null;
            var OnSqlGeneration = _getEventValue(ctx, "OnSqlGeneration") == null;
            var _connection = _getPropertyValue(ctx, typeof(Database), "_connection") == null;
            var _command = _getPropertyValue(ctx, typeof(Database), "_command") == null;
            var _configurationCheckPerformed = (bool)_getPropertyValue(ctx, typeof(Database), "_configurationCheckPerformed") == false;

            var _schematicManager = _getPropertyValue(ctx, typeof(DatabaseQuery), "SchematicFactory") == null;

            var OnBeforeSave = _getEventValue(ctx, "OnBeforeSave") == null;
            var OnAfterSave = _getEventValue(ctx, "OnAfterSave") == null;
            var OnSaving = _getEventValue(ctx, "OnSaving") == null;
            var OnConcurrencyViolation = _getEventValue(ctx, "OnConcurrencyViolation") == null;

            return Tables && _tableScriptMappings && ConnectionString && Reader && Configuration && OnSqlGeneration &&
                   _connection && _command && _configurationCheckPerformed && _schematicManager && OnBeforeSave &&
                   OnAfterSave && OnSaving && OnConcurrencyViolation;
        }

        public static bool Test_22(DbSqlContext ctx)
        {
            try
            {
                var names = new List<string> {"James","Megan"};
                var test = ctx.From<Contact>().Where(w => names.Contains(w.FirstName)).Count(w => w.ContactID == 10);
            }
            catch (Exception)
            {
                
                throw;
            }
            
            return true;
        }

        public static bool Test_23(DbSqlContext ctx)
        {
            try
            {
                // Testing renaming of FK Id and nullable FK
                var pizza = _addPizza(ctx);

                var test1 = pizza.Id != 0 && pizza.ToppingRenameId == pizza.Topping.Id && pizza.Topping.Id != 0;
                var test2 = pizza.Crust.Id == pizza.CrustId && pizza.CrustId != 0;
                var test3 = pizza.Crust.Topping.Id == pizza.Crust.ToppingId && pizza.Crust.ToppingId != 0;

                return (test1 && test2 && test3);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_24(DbSqlContext ctx)
        {
            try
            {
                // remove pizza with renamed FK id
                var pizza = _addPizza(ctx);

                ctx.Delete(pizza);

                var test1 = ctx.Find<Pizza>(pizza.Id) == null;
                var test2 = ctx.Find<Topping>(pizza.ToppingRenameId) == null;
                var test3 = ctx.Find<Crust>(pizza.CrustId) == null;
                var test4 = ctx.Find<Topping>(pizza.Crust.ToppingId) == null;

                return (test1 && test2 && test3 && test4);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_25(DbSqlContext ctx)
        {
            try
            {
                // Save with nullable foreign key
                var contact = new Contact
                {
                    CreatedBy = new User
                    {
                        Name = "James Demeuse"
                    },
                    EditedBy = new User
                    {
                        Name = "Different User"
                    },
                    FirstName = "Test",
                    LastName = "User",
                    Names = new List<Name>
                    {
                        new Name
                        {
                            Value = "Win!"
                        },
                        new Name
                        {
                            Value = "FTW!"
                        }
                    },
                    Appointments = new List<Appointment>
                    {
                        new Appointment
                        {
                            Description = "Appointment 1",
                            IsScheduled = false,
                            Address = new List<Address>
                            {
                                new Address
                                {
                                    Addy = "1234 First Ave South",
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
                        }
                    }
                };

                ctx.SaveChanges(contact);

                var that = ctx.Find<Contact>(contact.ContactID);

                return (that != null && that.Number == null);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_26(DbSqlContext ctx)
        {
            try
            {
                // Save with two PK's
                var linking = new Linking
                {
                    Description = "Test",
                    PolicyId = 10,
                    PolicyInfoId = 1
                };

                ctx.SaveChanges(linking);

                var that = ctx.Find<Linking>(10, 1);

                return (that != null);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_27(DbSqlContext ctx)
        {
            try
            {
                // Save with two PK's
                var linking = new Linking
                {
                    Description = "Test",
                    PolicyId = 10,
                    PolicyInfoId = 1
                };

                ctx.SaveChanges(linking);

                var that = ctx.Find<Linking>(10, 1);

                return (that != null);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_28(DbSqlContext ctx)
        {
            // saving in a lookup table
            var computer = new Computer
            {
                Name = "James PC",
                IsCustom = true,
                Processor = new Processor
                {
                    Name = "intel",
                    CoreType = CoreType.AMD,
                    Cores = 4,
                    Speed = 4
                },
                History = new List<History>
                {
                    new History
                    {
                        Description = "WINNING"
                    }
                }
            };

            ctx.SaveChanges(computer);

            var that = ctx.Find<Computer>(computer.Id);

            return (that != null && ctx.Find<Processor>(computer.ProcessorId) != null &&
                          (that.History == null || !that.History.Any(w => w.ComputerId == computer.Id)));
        }

        public static bool Test_29(DbSqlContext ctx)
        {
            // make sure the Lookup Table is directly editable,
            // IF FAIL!!!!!
            // Make sure the ComputerId exists first
            var history = new History
            {
                Description = "Winning",
                ComputerId = 2
            };

            ctx.SaveChanges(history);

            return (history.Id != 0);
        }

        public static bool Test_30(DbSqlContext ctx)
        {
            // making sure nullable types work
            var computer = new Computer
            {
                Name = "James PC",
                IsCustom = true,
                Processor = new Processor
                {
                    Name = "intel",
                    Cores = 4
                },
                History = new List<History>
                {
                    new History
                    {
                        Description = "WINNING"
                    }
                }
            };

            ctx.SaveChanges(computer);

            var that = ctx.Find<Computer>(computer.Id);

            ctx.Delete(computer);

            return (!that.Processor.Speed.HasValue && !that.Processor.CoreType.HasValue);
        }

        public static bool Test_31(DbSqlContext ctx)
        {
            try
            {
                // Should get errors when we try to set a value in
                // a timestamp column with an insert
                var history = new History
                {
                    Description = "Winning",
                    ComputerId = 2,
                    CreateDate = new byte[] { 0, 0, 0, 0, 68, 30 }
                };

                ctx.SaveChanges(history);

                return false;
            }
            catch (SqlSaveException)
            {
                return true;
            }
        }

        public static bool Test_32(DbSqlContext ctx)
        {
            // Make sure the any function works
            var policy = _addPolicy(ctx);

            var result = (ctx.From<Policy>().Any(w => w.Id == policy.Id));

            // cleanup
            ctx.Delete(policy);

            return result;
        }

        public static bool Test_33(DbSqlContext ctx)
        {
            // Make sure the any function works
            var policy = _addPolicy(ctx);

            var result = (ctx.From<Policy>().Any());

            // cleanup
            ctx.Delete(policy);

            return result;
        }

        public static bool Test_34(DbSqlContext ctx)
        {
            // Make sure the any function works
            var policy = _addPolicy(ctx);

            var result = (ctx.From<Policy>().Count() != 0);

            // cleanup
            ctx.Delete(policy);

            return result;
        }

        public static bool Test_35(DbSqlContext ctx)
        {
            try
            {
                // make sure first is working, should fail when no records found
                ctx.From<Policy>().First(w => w.Id == -1);

                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static bool Test_36(DbSqlContext ctx)
        {
            try
            {
                // make sure first finds records
                var policy = _addPolicy(ctx);

                var result = (ctx.From<Policy>().First(w => w.Id == policy.Id) != null);

                // cleanup
                ctx.Delete(policy);

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_37(DbSqlContext ctx)
        {
            try
            {
                // make sure first is working, should fail when no records found
                return (ctx.From<Policy>().FirstOrDefault(w => w.Id == -1) == null);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_38(DbSqlContext ctx)
        {
            try
            {
                // make sure first finds records
                var policy = _addPolicy(ctx);

                var result = (ctx.From<Policy>().FirstOrDefault(w => w.Id == policy.Id) != null);

                // cleanup
                ctx.Delete(policy);

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_39(DbSqlContext ctx)
        {
            // max should return the max id
            var max = ctx.From<Policy>().Select(w => w.Id).Max();

            return (max != 0);
        }

        public static bool Test_40(DbSqlContext ctx)
        {
            // min should return the min id
            var min = ctx.From<Policy>().Select(w => w.Id).Min();

            return (min != 0);
        }

        public static bool Test_41(DbSqlContext ctx)
        {
            // make sure order by is working
            var allItems = ctx.From<Policy>().OrderByDescending(w => w.Id).Take(2).ToList();

            return (allItems[0].Id > allItems[1].Id);
        }

        public static bool Test_42(DbSqlContext ctx)
        {
            // make sure order by is working
            var allItems = ctx.From<Policy>().OrderBy(w => w.Id).Take(2).ToList();

            return (allItems[1].Id > allItems[0].Id);
        }

        public static bool Test_43(DbSqlContext ctx)
        {
            //for (int i = 0; i < 1000; i++)
            //{
            //    _addPolicyWithPolicyInfo();
            //}

            // make sure then by is working
            var allItems = ctx.From<Policy>().OrderByDescending(w => w.Id).ThenBy(w => w.PolicyDate).Take(500).ToList();

            return (allItems[0].Id > allItems[499].Id && allItems[0].PolicyDate > allItems[499].PolicyDate);
        }

        public static bool Test_44(DbSqlContext ctx)
        {
            // make sure select is working properly
            var allItems = ctx.From<Policy>().Where(w => w.Id == 1).Select(w => w.InsuredName).First();

            return (!string.IsNullOrWhiteSpace(allItems));
        }

        public static bool Test_45(DbSqlContext ctx)
        {
            // individual method for take, even though it is test above
            var allItems = ctx.From<Policy>().Take(10).Select(w => w.InsuredName).ToList();

            return (allItems.Count == 10);
        }

        public static bool Test_46(DbSqlContext ctx)
        {
            // individual method for take, even though it is test above
            var allItems = ctx.From<Policy>().Select(w => w.PolicyInfoId).Distinct().ToList();

            return (allItems.Count > 0);
        }

        public static bool Test_47(DbSqlContext ctx)
        {
            // Should be able to select child of a child (not a list) and perform query from it
            var allItems = ctx.From<Contact>().Where(w => w.Number.PhoneType.Type == "Cell").ToList();

            return (allItems.Select(w => w.Number.PhoneType.Type).All(w => w == "Cell"));
        }

        public static bool Test_48(DbSqlContext ctx)
        {
            // Test Pseudo Keys
            //var artist = new Artist
            //{
            //    ActiveDate = DateTime.Now,
            //    Albums = new List<Album>
            //    {
            //        new Album
            //        {
            //            Name = "COOLE",
            //            TimesDownloaded = 1000
            //        },
            //        new Album
            //        {
            //            Name = "EP",
            //            TimesDownloaded = 165
            //        }
            //    },
            //    Agent = new Agent
            //    {
            //        Name = "James Demeuse"
            //    },
            //    FirstName = "Some",
            //    LastName = "Singer",
            //    Genre = "Country"
            //};

            //5,6

            //ctx.SaveChanges(artist);

            var artist1 = ctx.Find<Artist>(5);
            var artist2 = ctx.Find<Artist>(6);

            return (artist1 != null && artist2 != null && artist1.Albums.All(w => w.ArtistId == artist1.Id) &&
                    artist2.Albums.All(w => w.ArtistId == artist2.Id) && artist1.Agent.Id == artist1.AgentId &&
                    artist2.Agent.Id == artist2.AgentId);
        }

        public static bool Test_49(DbSqlContext ctx)
        {
            // try insert, happens when a table has only PK's
            var newRefId = ctx.From<TryInsert>().Select(w => w.RefId).Max() + 1;
            var newSomeId = ctx.From<TryInsert>().Select(w => w.SomeId).Max() + 1;

            var item = new TryInsert
            {
                RefId = newRefId,
                SomeId = newSomeId
            };

            var anyTryOne = ctx.From<TryInsert>().Any(w => w.RefId == newRefId && w.SomeId == newSomeId);

            ctx.SaveChanges(item);

            var anyTryTwo = ctx.From<TryInsert>().Any(w => w.RefId == newRefId && w.SomeId == newSomeId);

            return (!anyTryOne && anyTryTwo);
        }

        public static bool Test_50(DbSqlContext ctx)
        {
            // try insert, nothing should be inserted
            var newRefId = ctx.From<TryInsert>().Select(w => w.RefId).Max();
            var newSomeId = ctx.From<TryInsert>().Select(w => w.SomeId).Max();

            var item = new TryInsert
            {
                RefId = newRefId,
                SomeId = newSomeId
            };

            ctx.SaveChanges(item);

            return (ctx.From<TryInsert>().Any(w => w.RefId == newRefId && w.SomeId == newSomeId));
        }

        public static bool Test_51(DbSqlContext ctx)
        {
            // try insert update
            var newId = ctx.From<TryInsertUpdateWithGeneration>().Select(w => w.Id).Max() + 1;

            var item = new TryInsertUpdateWithGeneration
            {
                Id = newId,
                Name = "Testing"
            };

            ctx.SaveChanges(item);

            return (ctx.From<TryInsertUpdateWithGeneration>().Any(w => w.Id == newId) &&
                item.SequenceNumber != 0 &&
                item.OtherNumber != 0);
        }

        public static bool Test_52(DbSqlContext ctx)
        {
            try
            {
                // performs a try insert update
                var max1 = ctx.From<Linking>().Select(w => w.PolicyId).Max() + 1;
                var max2 = ctx.From<Linking>().Select(w => w.PolicyInfoId).Max() + 1;

                // make sure inserting and updating is working for the try insert update
                var linking = new Linking
                {
                    Description = "Test",
                    PolicyId = max1,
                    PolicyInfoId = max2
                };

                ctx.SaveChanges(linking);

                var that = ctx.Find<Linking>(max1, max2);

                that.Description = "Changed!";

                ctx.SaveChanges(that);

                that = ctx.Find<Linking>(max1, max2);

                var changed = that.Description;

                return (that != null && changed == "Changed!");
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_53(DbSqlContext ctx)
        {
            // try insert update should fail if the PK(s) are 0
            var item = new TryInsertUpdateWithGeneration
            {
                Id = 0,
                Name = "Testing",
                OtherNumber = 10
            };

            try
            {
                ctx.SaveChanges(item);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_54(DbSqlContext ctx)
        {
            // Update should be performed even if the id doesnt exist
            var item = new TestDefaultInsert
            {
                Name = "Test",
                Id = 100000
            };

            try
            {
                ctx.SaveChanges(item);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_55(DbSqlContext ctx)
        {
            // should use the db default and load it back into the model
            var item = new TestDefaultInsert
            {
                Name = "Test",
            };

            ctx.SaveChanges(item);

            return (item.Uid != Guid.Empty);
        }

        public static bool Test_56(DbSqlContext ctx)
        {
            // test update
            var contact = _addContact(ctx);

            contact.FirstName = "Joe Blow";

            ctx.SaveChanges(contact);

            contact = ctx.Find<Contact>(contact.ContactID);

            return (contact.FirstName == "Joe Blow");
        }

        public static bool Test_57(DbSqlContext ctx)
        {
            var testOne = true;
            var testTwo = true;
            var testThree = true;

            // test insert
            var item = new TestUpdateWithKeyDbGenerationOptionNone
            {
                Name = "Name",
                Id = 0
            };

            try
            {
                ctx.SaveChanges(item);
            }
            catch (Exception)
            {
                testOne = false;
            }

            item.Id = ctx.From<TestUpdateWithKeyDbGenerationOptionNone>().Select(w => w.Id).Max() + 1;

            ctx.SaveChanges(item);

            item = ctx.Find<TestUpdateWithKeyDbGenerationOptionNone>(item.Id);

            testTwo = item != null;

            item.Name = "NEW NAME!";

            ctx.SaveChanges(item);

            item = ctx.Find<TestUpdateWithKeyDbGenerationOptionNone>(item.Id);

            testThree = item.Name == "NEW NAME!";

            return (testOne && testTwo && testThree);
        }

        public static bool Test_58(DbSqlContext ctx)
        {
            var item = new TestKeyWithDbDefaultGeneration
            {
                Name = "Name"
            };

            try
            {
                ctx.SaveChanges(item);
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static bool Test_59(DbSqlContext ctx)
        {
            // schema changes
            var item = new SchemaChangeOne_dbo
            {
                Name = "Name"
            };

            ctx.SaveChanges(item);

            item = ctx.Find<SchemaChangeOne_dbo>(item.Id);

            return (item != null);
        }

        public static bool Test_60(DbSqlContext ctx)
        {
            // schema changes for normal insert that generates a pk
            var item = new SchemaChangeOne_ts
            {
                Name = "Name"
            };

            ctx.SaveChanges(item);

            item = ctx.Find<SchemaChangeOne_ts>(item.Id);

           return (item != null);
        }

        public static bool Test_61(DbSqlContext ctx)
        {
            // schema changes for try insert update
            var newId = ctx.From<TestUpdateWithKeyDbGenerationOptionNone_ts>().Select(w => w.Id).Max() + 1;

            // test insert
            var item = new TestUpdateWithKeyDbGenerationOptionNone_ts
            {
                Name = "Name",
                Id = newId
            };

            ctx.SaveChanges(item);

            item = ctx.Find<TestUpdateWithKeyDbGenerationOptionNone_ts>(item.Id);

            return (item != null);
        }

        public static bool Test_62(DbSqlContext ctx)
        {
            // test updates to tables in different schema
            var item = new TestUpdateNewSchema
            {
                Name = "Name"
            };

            ctx.SaveChanges(item);

            item = ctx.Find<TestUpdateNewSchema>(item.Id);

            item.Name = "NEW NAME!";

            ctx.SaveChanges(item);

            item = ctx.Find<TestUpdateNewSchema>(item.Id);

            return (item.Name == "NEW NAME!");
        }

        public static bool Test_63(DbSqlContext ctx)
        {
            try
            {
                // previous issue
                var count = ctx.From<Contact>().Where(w => w.FirstName.Contains("Test")).Count(w => w.LastName.Contains("Use"));

                return count > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_64(DbSqlContext ctx)
        {
            // make sure count comes back
            var test = ctx.From<Contact>().Where(w => w.FirstName.Contains("Te")).Count(w => w.ContactID == 10);

            return (test != 0);
        }

        public static bool Test_65(DbSqlContext ctx)
        {
            // save should fail because of foreign keys
            try
            {
                var contact = new Contact
                {
                    FirstName = "Test",
                    LastName = "User"
                };

                ctx.SaveChanges(contact);

                return false;
            }
            catch (SqlSaveException ex)
            {
                return true;
            }
        }

        public static bool Test_66(DbSqlContext ctx)
        {
            // save timestamp test
            var history = new History
            {
                Description = "Winning",
                ComputerId = 2
            };

            ctx.SaveChanges(history);

            return (history.CreateDate != null);
        }

        public static bool Test_67(DbSqlContext ctx)
        {
            try
            {
                var policy = new Policy
                {
                    County = "Hennepin",
                    CreatedBy = "James Demeuse",
                    CreatedDate = DateTime.Now,
                    FeeOwnerName = "OverTenCharactersLong",
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
            }
            catch (Exception ex)
            {
                return ex is SqlSaveException && ex.InnerException is MaxLengthException;
            }

            return false;
        }

        public static bool Test_68(DbSqlContext ctx)
        {
            try
            {
                var policy = new Policy
                {
                    County = "Hennepin",
                    CreatedBy = "James Demeuse",
                    CreatedDate = DateTime.Now,
                    FeeOwnerName = "Test",
                    FileNumber = 100,
                    InsuredName = "TruncateMEREMOVED",
                    PolicyAmount = 100,
                    PolicyDate = DateTime.Now,
                    PolicyInfoId = 1,
                    PolicyNumber = "001-8A",
                    StateID = 7,
                    UpdatedBy = "Me",
                    UpdatedDate = DateTime.Now
                };

                ctx.SaveChanges(policy);

                return policy.InsuredName == "TruncateME";
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_69(DbSqlContext ctx)
        {
            try
            {
                // make sure read only (throw exception) works
                var policy = new Policy_READONLY
                {
                    County = "Hennepin",
                    CreatedBy = "James Demeuse",
                    CreatedDate = DateTime.Now,
                    FeeOwnerName = "Test",
                    FileNumber = 100,
                    InsuredName = "fgh",
                    PolicyAmount = 100,
                    PolicyDate = DateTime.Now,
                    PolicyInfoId = 1,
                    PolicyNumber = "001-8A",
                    StateID = 7,
                    UpdatedBy = "Me",
                    UpdatedDate = DateTime.Now
                };

                ctx.SaveChanges(policy);

                return false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public static bool Test_70(DbSqlContext ctx)
        {
            try
            {
                var policy = new PolicyInfo_READONLY
                {
                    Description = "info",
                    FirstName = "james",
                    LastName = "demeuse",
                    Stamp = Guid.NewGuid()
                };

                ctx.SaveChanges(policy);

                return policy.Id == 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool Test_71(DbSqlContext ctx)
        {
            var t = ctx.From<Contact>().Where(w => w.ContactID == 1).Select(w => new
            {
                ID = w.ContactID,
                w.CreatedByUserID,
                w.FirstName,
                w.Number,
                w.Appointments,
                w.Number.PhoneType
            }).ToList();

            return t != null;
        }

        public static bool Test_72(DbSqlContext ctx)
        {
            var t = ctx.From<Contact>().Where(w => w.ContactID == 1).Select(w => new
            {
                w.ContactID,
                w.CreatedByUserID,
                w.FirstName,
                w.Number,
                w.Appointments,
                w.Number.PhoneType
            }).FirstOrDefault();

            return t != null;
        }

        public static bool Test_73(DbSqlContext ctx)
        {
            var t = ctx.From<Contact>().FirstOrDefault(w => w.FirstName != null);

            return t != null;
        }

        public static bool Test_74(DbSqlContext ctx)
        {
            var t = ctx.From<Contact>().FirstOrDefault(w => w.FirstName == null);

            return t != null;
        }

        public static bool Test_75(DbSqlContext ctx)
        {
            var t = ctx.From<Appointment>().FirstOrDefault(w => w.IsScheduled && w.Description == null);

            return t != null;
        }

        public static bool Test_76(DbSqlContext ctx)
        {
            var t =
                ctx.From<Contact>()
                    .FirstOrDefault(
                        w =>
                            w.FirstName.StartsWith("Te") &&
                            w.Appointments.Any(x => !x.IsScheduled && x.Description == null));

            return t != null;
        }

        public static bool Test_77(DbSqlContext ctx)
        {
            var t =
                ctx.From<Contact>()
                    .FirstOrDefault(
                        w =>
                            w.FirstName.StartsWith("Te") &&
                            w.Appointments.Any(x => x.IsScheduled));

            return t != null;
        }

        public static bool Test_78(DbSqlContext ctx)
        {
            try
            {
                ctx.From<Policy>().Select(w => w.Test).FirstOrDefault();

                return false;
            }
            catch (QueryNotValidException ex)
            {
                return true;
            }
        }

        public static bool Test_79(DbSqlContext ctx)
        {
            try
            {
                ctx.From<Policy>()
                        .FirstOrDefault(w => w.Test == "");

                return false;
            }
            catch (QueryNotValidException ex)
            {
                return true;
            }
        }

        #region helpers
        protected static Policy _addPolicy(DbSqlContext ctx)
        {
            var policy = new Policy
            {
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

            return policy;
        }

        protected static Pizza _addPizza(DbSqlContext ctx)
        {
            var pizza = new Pizza
            {
                CookTime = 35,
                Name = "Deep Dish",
                Topping = new Topping
                {
                    Cost = .20m,
                    Name = "Onion"
                },
                Crust = new Crust
                {
                    Name = "Stuffed",
                    Topping = new Topping
                    {
                        Cost = 1.35m,
                        Name = "Butter, Cheese"
                    }
                },
                DeliveryMan = new DeliveryMan
                {
                    AverageDeliveryTime = 15,
                    FirstName = "James",
                    LastName = "Demeuse",
                }
            };

            ctx.SaveChanges(pizza);

            return pizza;
        }

        protected static Contact _addContact(DbSqlContext ctx)
        {
            var contact = new Contact
            {
                CreatedBy = new User
                {
                    Name = "James Demeuse"
                },
                EditedBy = new User
                {
                    Name = "Different User"
                },
                FirstName = "Test",
                LastName = "User",
                Names = new List<Name>
                {
                    new Name
                    {
                        Value = "Win!"
                    },
                    new Name
                    {
                        Value = "FTW!"
                    }
                },
                Number = new PhoneNumber
                {
                    Phone = "555-555-5555",
                    PhoneType = new PhoneType
                    {
                        Type = "Cell"
                    }
                },
                Appointments = new List<Appointment>
                {
                    new Appointment
                    {
                        Description = "Appointment 1",
                        IsScheduled = false,
                        Address = new List<Address>
                        {
                            new Address
                            {
                                Addy = "1234 First Ave South",
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
                    }
                }
            };

            ctx.SaveChanges(contact);

            return contact;
        }

        private static void _deleteAllData(DbSqlContext ctx)
        {
            const string sql = @"
    Delete From [User];
    Delete From Name;
    Delete From ZipCode;
    Delete From [Address];
    Delete From StateCode;
    Delete From Appointments;
    Delete From Contacts;
    Delete From PhoneType;
    Delete From PhoneNumbers;
";
        }

        private static KeyValuePair<Policy, PolicyInfo> _addPolicyWithPolicyInfo(DbSqlContext ctx)
        {
            var policyInfo = new PolicyInfo
            {
                Description = "info",
                FirstName = "james",
                LastName = "demeuse",
                Stamp = Guid.NewGuid()
            };

            ctx.SaveChanges(policyInfo);

            var policy = new Policy
            {
                County = "Hennepin",
                CreatedBy = "James Demeuse",
                CreatedDate = DateTime.Now,
                FeeOwnerName = "Test",
                FileNumber = 100,
                InsuredName = "James Demeuse",
                PolicyAmount = 100,
                PolicyDate = DateTime.Now,
                PolicyInfoId = policyInfo.Id,
                PolicyNumber = "001-8A",
                StateID = 7,
                UpdatedBy = "Me",
                UpdatedDate = DateTime.Now,
            };

            ctx.SaveChanges(policy);

            return new KeyValuePair<Policy, PolicyInfo>(policy, policyInfo);
        }

        private static List<Policy> _addPolicies(DbSqlContext ctx)
        {
            var result = new List<Policy>();

            for (var i = 0; i < 100; i++)
            {
                result.Add(_addPolicy(ctx));
            }

            return result;
        }

        private static object _getPropertyValue(object entity, string propertyName)
        {
            var property = entity.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

            return property.GetValue(entity);
        }

        private static object _getEventValue(object entity, string propertyName)
        {
            var e = entity.GetType().GetEvent(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

            return e.GetRaiseMethod();
        }

        private static object _getPropertyValue(object entity, Type type, string propertyName)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

            return property.GetValue(entity);
        }
        #endregion
    }
}
