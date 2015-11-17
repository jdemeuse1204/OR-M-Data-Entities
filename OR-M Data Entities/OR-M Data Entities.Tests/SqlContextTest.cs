using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Scripts;
using OR_M_Data_Entities.Tests.Tables;

namespace OR_M_Data_Entities.Tests
{
    public static class Extensions
    {
        public static ConnectionState GetConnectionState(this DbSqlContext context)
        {
            var connectionProperty = context.GetType().GetProperty("Connection", BindingFlags.Instance | BindingFlags.NonPublic);
            return ((SqlConnection)connectionProperty.GetValue(context)).State;
        }
    }

    [TestClass]
    public class SqlContextTest
    {
        private readonly DbSqlContext ctx = new DbSqlContext("sqlExpress");

        [TestMethod]
        public void Test_1()
        {
            // Disconnnect Test With Execute Query
            ctx.ExecuteQuery("Select Top 1 1");

            var state = ctx.GetConnectionState();

            Assert.AreEqual(state, ConnectionState.Closed);
            // connection should close when query is done executing
        }

        [TestMethod]
        public void Test_2()
        {
            // Disconnnect Test With Execute Query
            var result = ctx.ExecuteQuery<int>("Select Top 1 1").First();

            var state = ctx.GetConnectionState();

            Assert.AreEqual(state, ConnectionState.Closed);
            // connection should close when query is done executing
        }

        [TestMethod]
        public void Test_3()
        {
            // Add one record, no foreign keys
            // make sure it has an id
            var policy = _addPolicy();

            Assert.IsTrue(policy.Id != 0);
        }

        [TestMethod]
        public void Test_4()
        {
            // Test find method
            var policy = _addPolicy();

            var foundEntity = ctx.Find<Policy>(policy.Id);

            Assert.IsTrue(foundEntity != null);
        }

        [TestMethod]
        public void Test_5()
        {
            // Test first or default method
            var policy = _addPolicy();

            var foundEntity = ctx.From<Policy>().FirstOrDefault(w => w.Id == policy.Id);

            Assert.IsTrue(foundEntity != null);
        }

        [TestMethod]
        public void Test_6()
        {
            // Test where with first or default method
            var policy = _addPolicy();

            var foundEntity = ctx.From<Policy>().Where(w => w.Id == policy.Id).FirstOrDefault();

            Assert.IsTrue(foundEntity != null);
        }

        [TestMethod]
        public void Test_7()
        {
            // Delete one record, no foreign keys
            var policy = _addPolicy();

            ctx.Delete(policy);

            var foundEntity = ctx.Find<Policy>(policy.Id);

            Assert.IsTrue(foundEntity == null);
        }

        [TestMethod]
        public void Test_8()
        {
            // Test Disconnect with expression query
            var policy = _addPolicy();

            var foundEntity = ctx.From<Policy>().Where(w => w.Id == policy.Id).FirstOrDefault();

            var state = ctx.GetConnectionState();

            Assert.AreEqual(state, ConnectionState.Closed);
        }

        [TestMethod]
        public void Test_9()
        {
            // Test contains in expression query
            var policies = _addPolicies().Select(w => w.Id).ToList();

            var foundEntities = ctx.From<Policy>().ToList(w => policies.Contains(w.Id));

            Assert.AreEqual(policies.Count, foundEntities.Count);
        }

        [TestMethod]
        public void Test_10()
        {
            // Test inner join
            var policies = _addPolicyWithPolicyInfo();

            var policy = ctx.From<Policy>()
                .InnerJoin(
                    ctx.From<PolicyInfo>(),
                    p => p.PolicyInfoId,
                    pi => pi.Id,
                    (p, pi) => p).FirstOrDefault(w => w.Id == policies.Key.Id);

            Assert.AreEqual(policy.Id, policies.Key.Id);
        }

        [TestMethod]
        public void Test_11()
        {
            // Test inner join
            var policies = _addPolicyWithPolicyInfo();

            var policy = ctx.From<Policy>()
                .LeftJoin(
                    ctx.From<PolicyInfo>(),
                    p => p.PolicyInfoId,
                    pi => pi.Id,
                    (p, pi) => p).FirstOrDefault(w => w.Id == policies.Key.Id);

            Assert.AreEqual(policy.Id, policies.Key.Id);
        }

        [TestMethod]
        public void Test_12()
        {
            // Test connection opening and closing
            for (var i = 0; i < 100; i++)
            {
                var item = ctx.From<Policy>().FirstOrDefault(w => w.Id == 1);

                if (item != null)
                {
                }
            }

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_13()
        {
            // Disconnnect Test With Execute Script
            ctx.ExecuteScript<int>(new CustomScript1()).First();

            var state = ctx.GetConnectionState();

            Assert.AreEqual(state, ConnectionState.Closed);
            // connection should close when query is done executing
        }

        [TestMethod]
        public void Test_14()
        {
            // Disconnnect Test With Execute Script
            ctx.ExecuteScript(new CustomScript2());

            var state = ctx.GetConnectionState();

            Assert.AreEqual(state, ConnectionState.Closed);
            // connection should close when query is done executing
        }

        [TestMethod]
        public void Test_15()
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

            Assert.IsTrue(phoneNumber.ID != 0 && phoneNumber.PhoneType.ID != 0 &&
                          phoneNumber.PhoneTypeID == phoneNumber.PhoneType.ID);
        }

        [TestMethod]
        public void Test_16()
        {
            // test entity state
            var phoneType = new PhoneType
            {
                Type = "Cell"
            };

            var beforeSave = phoneType.GetState();

            ctx.SaveChanges(phoneType);

            var afterSave = phoneType.GetState();

            Assert.IsTrue(beforeSave == EntityState.Modified && afterSave == EntityState.UnChanged);
        }

        [TestMethod]
        public void Test_17()
        {
            // insert complex object
            var contact = _addContact();

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

            Assert.IsTrue(test1 && test2 && test3 && test4 && test5 && test6 && test7 && test8 && test9 && test10 &&
                          test11 && test12 && test13 && test14);
        }


        [TestMethod]
        public void Test_18()
        {
            // delete complex object
            var contact = _addContact();

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

            Assert.IsTrue(test1 && test2 && test3 && test4 && test5 && test6 && test7);
        }

        [TestMethod]
        public void Test_19()
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

                Assert.IsTrue(false);
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void Test_20()
        {
            // check to see if we can read from a readonly table
            var person = ctx.Find<Person>(2);

            Assert.IsNotNull(person);
        }

        [TestMethod]
        public void Test_21()
        {
            var newContact = _addContact();
            // test view one table
            var contact = ctx.FromView<Contact>("ContactOnly").FirstOrDefault(w => w.ContactID == newContact.ContactID);

            ctx.Delete(newContact);

            Assert.IsTrue(contact.Appointments == null && contact.CreatedBy == null && contact.EditedBy == null &&
                             contact.Names == null && contact.Number == null);
        }

        [TestMethod]
        public void Test_22()
        {
            var newContact = _addContact();
            // test view two tables
            var contact = ctx.FromView<Contact>("ContactAndPhone").FirstOrDefault(w => w.ContactID == newContact.ContactID);

            ctx.Delete(newContact);

            Assert.IsTrue(contact.Appointments == null && contact.CreatedBy == null && contact.EditedBy == null &&
                             contact.Names == null && contact.Number != null);
        }

        [TestMethod]
        public void Test_23()
        {
            try
            {
                // Testing renaming of FK Id and nullable FK
                var pizza = _addPizza();

                var test1 = pizza.Id != 0 && pizza.ToppingRenameId == pizza.Topping.Id && pizza.Topping.Id != 0;
                var test2 = pizza.Crust.Id == pizza.CrustId && pizza.CrustId != 0;
                var test3 = pizza.Crust.Topping.Id == pizza.Crust.ToppingId && pizza.Crust.ToppingId != 0;

                Assert.IsTrue(test1 && test2 && test3);
            }
            catch (Exception)
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void Test_24()
        {
            try
            {
                // remove pizza with renamed FK id
                var pizza = _addPizza();

                ctx.Delete(pizza);

                var test1 = ctx.Find<Pizza>(pizza.Id) == null;
                var test2 = ctx.Find<Topping>(pizza.ToppingRenameId) == null;
                var test3 = ctx.Find<Crust>(pizza.CrustId) == null;
                var test4 = ctx.Find<Topping>(pizza.Crust.ToppingId) == null;

                Assert.IsTrue(test1 && test2 && test3 && test4);
            }
            catch (Exception)
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void Test_25()
        {
            try
            {
                // Save and retrieve with a null FK
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
                        Name = "Stuffed"
                    },
                    DeliveryMan = new DeliveryMan
                    {
                        AverageDeliveryTime = 15,
                        FirstName = "James",
                        LastName = "Demeuse",
                        CreateDate = BitConverter.GetBytes(DateTime.Now.ToOADate())
                    }
                };

                ctx.SaveChanges(pizza);

                var that = ctx.Find<Pizza>(pizza.Id);

                Assert.IsTrue(that != null && that.Crust.Topping == null);
            }
            catch (Exception)
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void Test_26()
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

                Assert.IsTrue(that != null);
            }
            catch (Exception)
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void Test_27()
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

                Assert.IsTrue(that != null);
            }
            catch (Exception)
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void Test_28()
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

            Assert.IsTrue(that != null && ctx.Find<Processor>(computer.ProcessorId) != null &&
                          (that.History == null || !that.History.Any(w => w.ComputerId == computer.Id)));
        }

        [TestMethod]
        public void Test_29()
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

            Assert.IsTrue(history.Id != 0);
        }

        [TestMethod]
        public void Test_30()
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

            Assert.IsTrue(!that.Processor.Speed.HasValue && !that.Processor.CoreType.HasValue);
        }

        [TestMethod]
        public void Test_31()
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

            Assert.IsTrue(state0 == EntityState.Modified && state1 == EntityState.Modified &&
                          state2 == EntityState.UnChanged && state3 == EntityState.UnChanged &&
                          state4 == EntityState.Modified);
        }

        [TestMethod]
        public void Test_32()
        {
            // Make sure the any function works
            var policy = _addPolicy();

            Assert.IsTrue(ctx.From<Policy>().Any(w => w.Id == policy.Id));

            // cleanup
            ctx.Delete(policy);
        }

        [TestMethod]
        public void Test_33()
        {
            // Make sure the any function works
            var policy = _addPolicy();

            Assert.IsTrue(ctx.From<Policy>().Any());

            // cleanup
            ctx.Delete(policy);
        }

        [TestMethod]
        public void Test_34()
        {
            // Make sure the any function works
            var policy = _addPolicy();

            Assert.IsTrue(ctx.From<Policy>().Count() != 0);

            // cleanup
            ctx.Delete(policy);
        }

        [TestMethod]
        public void Test_35()
        {
            try
            {
                // make sure first is working, should fail when no records found
                ctx.From<Policy>().First(w => w.Id == -1);

                Assert.IsTrue(false);
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void Test_36()
        {
            try
            {
                // make sure first finds records
                var policy = _addPolicy();

                Assert.IsTrue(ctx.From<Policy>().First(w => w.Id == policy.Id) != null);

                // cleanup
                ctx.Delete(policy);

            }
            catch (Exception)
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void Test_37()
        {
            try
            {
                // make sure first is working, should fail when no records found
                Assert.IsTrue(ctx.From<Policy>().FirstOrDefault(w => w.Id == -1) == null);
            }
            catch (Exception)
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void Test_38()
        {
            try
            {
                // make sure first finds records
                var policy = _addPolicy();

                Assert.IsTrue(ctx.From<Policy>().FirstOrDefault(w => w.Id == policy.Id) != null);

                // cleanup
                ctx.Delete(policy);
            }
            catch (Exception)
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void Test_39()
        {
            // max should return the max id
            var max = ctx.From<Policy>().Select(w => w.Id).Max();

            Assert.IsTrue(max != 0);
        }

        [TestMethod]
        public void Test_40()
        {
            // min should return the min id
            var max = ctx.From<Policy>().Select(w => w.Id).Min();

            Assert.IsTrue(max != 0);
        }

        [TestMethod]
        public void Test_41()
        {
            // make sure order by is working
            var allItems = ctx.From<Policy>().OrderByDescending(w => w.Id).Take(2).ToList();

            Assert.IsTrue(allItems[0].Id > allItems[1].Id);
        }

        [TestMethod]
        public void Test_42()
        {
            // make sure order by is working
            var allItems = ctx.From<Policy>().OrderBy(w => w.Id).Take(2).ToList();

            Assert.IsTrue(allItems[1].Id > allItems[0].Id);
        }

        [TestMethod]
        public void Test_43()
        {
            //for (int i = 0; i < 1000; i++)
            //{
            //    _addPolicyWithPolicyInfo();
            //}

            // make sure then by is working
            var allItems = ctx.From<Policy>().OrderByDescending(w => w.Id).ThenBy(w => w.PolicyDate).Take(500).ToList();

            Assert.IsTrue(allItems[0].Id > allItems[499].Id && allItems[0].PolicyDate > allItems[499].PolicyDate);
        }

        [TestMethod]
        public void Test_44()
        {
            // make sure select is working properly
            var allItems = ctx.From<Policy>().Where(w => w.Id == 1).Select(w => w.InsuredName).First();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(allItems));
        }


        [TestMethod]
        public void Test_45()
        {
            // individual method for take, even though it is test above
            var allItems = ctx.From<Policy>().Take(10).Select(w => w.InsuredName).ToList();

            Assert.IsTrue(allItems.Count == 10);
        }

        [TestMethod]
        public void Test_46()
        {
            // individual method for take, even though it is test above
            var allItems = ctx.From<Policy>().Select(w => w.PolicyInfoId).Distinct().ToList();

            Assert.IsTrue(allItems.Count > 0);
        }

        [TestMethod]
        public void Test_47()
        {
            // Should be able to select child of a child (not a list) and perform query from it
            var allItems = ctx.From<Contact>().Where(w => w.Number.PhoneType.Type == "Cell").ToList();

            Assert.IsTrue(allItems.Select(w => w.Number.PhoneType.Type).All(w => w == "Cell"));
        }

        [TestMethod]
        public void Test_48()
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

            Assert.IsTrue(artist1 != null && artist2 != null && artist1.Albums.All(w => w.ArtistId == artist1.Id) &&
                          artist2.Albums.All(w => w.ArtistId == artist2.Id) && artist1.Agent.Id == artist1.AgentId &&
                          artist2.Agent.Id == artist2.AgentId);
        }

        [TestMethod]
        public void Test_49()
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

            Assert.IsTrue(!anyTryOne && anyTryTwo);
        }

        [TestMethod]
        public void Test_50()
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

            Assert.IsTrue(ctx.From<TryInsert>().Any(w => w.RefId == newRefId && w.SomeId == newSomeId));
        }

        [TestMethod]
        public void Test_51()
        {
            // try insert update
            var newId = ctx.From<TryInsertUpdateWithGeneration>().Select(w => w.Id).Max() + 1;

            var item = new TryInsertUpdateWithGeneration
            {
                Id = newId,
                Name = "Testing"
            };

            ctx.SaveChanges(item);

            Assert.IsTrue(ctx.From<TryInsertUpdateWithGeneration>().Any(w => w.Id == newId) &&
                item.SequenceNumber != 0 &&
                item.OtherNumber != 0);
        }

        [TestMethod]
        public void Test_52()
        {
            // try insert update, cannot update identity column
            var newId = ctx.From<TryInsertUpdateWithGeneration>().Select(w => w.Id).Max();

            var item = new TryInsertUpdateWithGeneration
            {
                Id = newId,
                Name = "Testing",
                OtherNumber = 10
            };

            try
            {
                ctx.SaveChanges(item);
                Assert.IsTrue(false);
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void Test_53()
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
                Assert.IsTrue(false);
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void Test_54()
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
                Assert.IsTrue(true);
            }
            catch (Exception)
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void Test_55()
        {
            // should use the db default and load it back into the model
            var item = new TestDefaultInsert
            {
                Name = "Test",
            };

            ctx.SaveChanges(item);
            
            Assert.IsTrue(item.Uid != Guid.Empty);
        }

        [TestMethod]
        public void Test_56()
        {
            // test update
            var contact = _addContact();

            contact.FirstName = "Joe Blow";

            ctx.SaveChanges(contact);

            contact = ctx.Find<Contact>(contact.ContactID);

            Assert.IsTrue(contact.FirstName == "Joe Blow");
        }

        [TestMethod]
        public void Test_57()
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
                testOne = false;
            }
            catch (Exception)
            {
                
            }

            item.Id = ctx.From<TestUpdateWithKeyDbGenerationOptionNone>().Select(w => w.Id).Max() + 1;

            ctx.SaveChanges(item);

            item = ctx.Find<TestUpdateWithKeyDbGenerationOptionNone>(item.Id);

            testTwo = item != null;

            item.Name = "NEW NAME!";

            ctx.SaveChanges(item);

            item = ctx.Find<TestUpdateWithKeyDbGenerationOptionNone>(item.Id);

            testThree = item.Name == "NEW NAME!";

            Assert.IsTrue(testOne && testTwo && testThree);
        }

        [TestMethod]
        public void Test_58()
        {
            var item = new TestKeyWithDbDefaultGeneration
            {
                Name = "Name"
            };

            try
            {
                ctx.SaveChanges(item);
                Assert.IsTrue(false);
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void Test_59()
        {
            // schema changes
            var item = new SchemaChangeOne_dbo
            {
                Name = "Name"
            };

            ctx.SaveChanges(item);

            item = ctx.Find<SchemaChangeOne_dbo>(item.Id);

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_60()
        {
            // schema changes for normal insert that generates a pk
            var item = new SchemaChangeOne_ts
            {
                Name = "Name"
            };

            ctx.SaveChanges(item);

            item = ctx.Find<SchemaChangeOne_ts>(item.Id);

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_61()
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
            
            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_62()
        {
            // test updates to tables in different schema
            var item = new TestUpdateNewSchema
            {
                Name = "Name"
            };

            ctx.SaveChanges(item);

            item.Name = "NEW NAME!";

            ctx.SaveChanges(item);

            item = ctx.Find<TestUpdateNewSchema>(item.Id);

            Assert.IsTrue(item.Name == "NEW NAME!");
        }

        [TestMethod]
        public void Test_63()
        {
            try
            {
                // previous issue
                var test = ctx.From<Contact>().Where(w => w.FirstName.Contains("Win")).Count(w => w.ContactID == 10);
                Assert.IsTrue(true);
            }
            catch (Exception)
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void Test_64()
        {
            // make sure count comes back
            var test = ctx.From<Contact>().Where(w => w.FirstName.Contains("Te")).Count(w => w.ContactID == 10);

            Assert.IsTrue(test != 0);
        }

        [TestMethod]
        public void Test_65()
        {
            try
            {
                var contact = new Contact
                {
                    FirstName = "Test",
                    LastName = "User"
                };

                ctx.SaveChanges(contact);

                if (contact != null)
                {
                    contact.Appointments = null;
                }

                Assert.IsTrue(false);
            }
            catch (Exception)
            {
               Assert.IsTrue(true);
            }
        }

        #region helpers
        private Policy _addPolicy()
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

        private Pizza _addPizza()
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
                    CreateDate = BitConverter.GetBytes(DateTime.Now.ToOADate())
                }
            };

            ctx.SaveChanges(pizza);

            return pizza;
        }

        private Contact _addContact()
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

        private void _deleteAllData()
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

        private KeyValuePair<Policy, PolicyInfo> _addPolicyWithPolicyInfo()
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

        private List<Policy> _addPolicies()
        {
            var result = new List<Policy>();

            for (var i = 0; i < 100; i++)
            {
                result.Add(_addPolicy());
            }

            return result;
        }

        #endregion
    }

    public class CustomScript1 : CustomScript<int>
    {
        protected override string Sql
        {
            get { return "Select Top 1 1"; }
        }
    }

    public class CustomScript2 : CustomScript
    {
        protected override string Sql
        {
            get { return "Select Top 1 1"; }
        }
    }

    public class ScalarFunction1 : ScalarFunction<int>
    {
        
    }
}
