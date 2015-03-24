using System;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.Transform;
using OR_M_Data_Entities.Tests.Context;
using OR_M_Data_Entities.Tests.Tables;

namespace OR_M_Data_Entities.Tests
{
    [TestClass]
    public class SqlContextTest
    {
        #region Fields
        private readonly SqlContext sqlContext = new SqlContext();
        private readonly EntityContext entityContext = new EntityContext();
        private readonly int TestID = 63;
        #endregion

        #region Entity Context Tests

        #region Fetch
        [TestMethod]
        public void Test_Entity_All()
        {
            var allPolicies = entityContext.Policies.All();

            Assert.IsTrue(allPolicies != null && allPolicies.Count > 0);
        }

        [TestMethod]
        public void Test_Entity_Where_ListCompare()
        {
            var ids = entityContext.Policies.All().Take(10).Select(w => w.Id).ToList();

            var allPolicies = entityContext.Policies.Where(w => ids.Contains(w.Id));

            Assert.IsTrue(allPolicies != null && allPolicies.Count == ids.Count);
        }

        [TestMethod]
        public void Test_Entity_Where_Linq()
        {
            var item = (from a in entityContext.Policies
                        where a.Id == TestID
                        select a);

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Entity_Where()
        {
            var item = entityContext.Policies.Where(w => w.Id == TestID).FirstOrDefault();

            Assert.IsTrue(item != null);
        }
        #endregion

        #region Save And Delete
        [TestMethod]
        public void Test_Entity_Save()
        {
            var policy = new Policy
            {
                CreatedBy = "James Demeuse",
                CreatedDate = DateTime.Now,
                UpdatedBy = "James Demeuse",
                UpdatedDate = DateTime.Now,
                FeeOwnerName = "James Demeuse",
                InsuredName = "Branden Purdy",
                County = "Minnesota",
                PolicyDate = DateTime.Now,
            };

            entityContext.Policies.Add(policy);
            entityContext.SaveChanges();

            Assert.IsTrue(policy.Id != 0);
        }

        [TestMethod]
        public void Test_Entity_Delete()
        {
            var policy = new Policy
            {
                CreatedBy = "James Demeuse",
                CreatedDate = DateTime.Now,
                UpdatedBy = "James Demeuse",
                UpdatedDate = DateTime.Now,
                FeeOwnerName = "James Demeuse",
                InsuredName = "Branden Purdy",
                County = "Minnesota",
                PolicyDate = DateTime.Now,
            };

            entityContext.Policies.Add(policy);
            entityContext.SaveChanges();

            entityContext.Policies.Remove(policy);
            entityContext.SaveChanges();

            var item = entityContext.Policies.Where(w => w.Id == policy.Id).FirstOrDefault();

            Assert.IsTrue(item == null);
        }
        #endregion

        #endregion


        #region Sql Context Tests

        #region Fetch

        #region Generic
        [TestMethod]
        public void Test_Sql_All()
        {
            var allPolicies = sqlContext.All<Policy>();

            Assert.IsTrue(allPolicies != null && allPolicies.Count > 0);
        }

        [TestMethod]
        public void Test_Sql_Find()
        {
            var item = sqlContext.Find<Policy>(TestID);

            Assert.IsTrue(item != null);
        }

        #endregion

        #region Data Reader
        [TestMethod]
        public void Test_Sql_DataReader_All()
        {
            var sql = "Select * From Policy";

            var items = sqlContext.ExecuteQuery<Policy>(sql).All();

            Assert.IsTrue(items != null && items.Count > 0);
        }

        [TestMethod]
        public void Test_Sql_DataReader_All_WithAutoLoad()
        {
            var c = new Contact
            {
                FirstName = "James",
                LastName = "Demeuse"
            };

            sqlContext.SaveChanges(c);

            var a1 = new Appointment
            {
                ContactID = c.ID,
                Description = "Test"
            };

            var a2 = new Appointment
            {
                ContactID = c.ID,
                Description = "Test"
            };

            sqlContext.SaveChanges(a1);
            sqlContext.SaveChanges(a2);

            var addy = new Address
            {
                Addy = "TEST ADDY",
                AppointmentID = a1.ID
            };

            sqlContext.SaveChanges(addy);

            var zip = new Zip
            {
                AddressID = addy.ID,
                Zip4 = "TEST"
            };

            sqlContext.SaveChanges(zip);           

            var sql = "Select * From Contacts";

            var items = sqlContext.ExecuteQuery<Contact>(sql).All();

            Assert.IsTrue(items != null && items.Count > 0);

            sqlContext.Delete(c);
            sqlContext.Delete(a1);
            sqlContext.Delete(a2);
            sqlContext.Delete(addy);
            sqlContext.Delete(zip);
        }

        [TestMethod]
        public void Test_Sql_DataReader_Select()
        {
            var sql = "Select * From Policy";

            var item = sqlContext.ExecuteQuery<Policy>(sql).Select();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_DataReader_Select_WithAutoLoad()
        {
            var sql = "Select Top 1 * From Contacts Where ID = 27";

            var item = sqlContext.ExecuteQuery<Contact>(sql).Select();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_DataReader_Loop_Foreach()
        {
            var sql = "Select * From Policy";
            var count = 0;

            foreach (var item in sqlContext.ExecuteQuery<Policy>(sql))
            {
                count++;
            }

            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        public void Test_Sql_DataReader_Linq()
        {
            var sql = "Select * From Policy";
            var count = sqlContext.ExecuteQuery<Policy>(sql).Cast<Policy>().Count();

            Assert.IsTrue(count > 0);
        }
        #endregion

        #region Expression Query
        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_First()
        {
            var item = sqlContext.Where<Policy>(w => w.Id == TestID).First<Policy>();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_First_MethodCallEquals()
        {
            var item = sqlContext.Where<Policy>(w => w.Id.Equals(TestID)).First<Policy>();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_First_MethodCallEndsWith()
        {
            var item = sqlContext.Where<Policy>(w => w.CreatedBy.EndsWith("Demeuse")).First<Policy>();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_First_MethodCallContains()
        {
            var item = sqlContext.Where<Policy>(w => w.CreatedBy.Contains("Demeuse")).All<Policy>();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_First_MethodCallStartsWith()
        {
            var item = sqlContext.Where<Policy>(w => w.CreatedBy.StartsWith("James")).First<Policy>();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_All()
        {
            var item = sqlContext.Where<Policy>(w => w.Id == TestID).All<Policy>();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_All_2()
        {
            var item = sqlContext.Where<Policy>(w => w.Id == TestID).All();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_First_2()
        {
            var item = sqlContext.Where<Policy>(w => w.Id == TestID).First();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_Distinct_First()
        {
            var item = sqlContext.Where<Policy>(w => w.Id == TestID).Distinct().First<Policy>();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_Join_First()
        {
            var item = sqlContext.From<Policy>()
                    .Join<Policy, PolicyInfo>((p, c) => p.PolicyInfoId == c.Id)
                    .Where<Policy>(w => w.Id == 45)
                    .Select<PolicyInfo>()
                    .First<PolicyInfo>();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_Join_ForeignKeys_First()
        {
            var contact = new Contact
            {
                FirstName = "James",
                LastName = "Demeuse"
            };

            sqlContext.SaveChanges(contact);

            var appointment = new Appointment
            {
                Description = "Win",
                ContactID = contact.ID
            };

            sqlContext.SaveChanges(appointment);

            var item = sqlContext.From<Contact>()
                    .Join<Contact, Appointment>()
                    .Where<Contact>(w => w.ID == contact.ID)
                    .Select<Appointment>()
                    .First<Appointment>();

            Assert.IsTrue(item != null);

            sqlContext.Delete(contact);
            sqlContext.Delete(appointment);
        }

        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_Cast()
        {
            var currentDateTime = DateTime.Now;
            var item = sqlContext.From<Policy>()
                .Where<Policy>(w => DbFunctions.Cast(w.CreatedDate, SqlDbType.Date) == DbFunctions.Cast(currentDateTime, SqlDbType.Date))
                .Select<Policy>()
                .First<Policy>();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_Convert()
        {
            var currentDateTime = DateTime.Now;
            var item = sqlContext.From<Policy>()
                .Where<Policy>(w => DbFunctions.Cast(w.CreatedDate, SqlDbType.Date) == DbFunctions.Cast(currentDateTime, SqlDbType.Date))
                .Select<Policy>(w => DbFunctions.Convert(SqlDbType.VarChar, w.CreatedDate, 101))
                .First<string>();

            Assert.IsTrue(item != null);
        }
        #endregion

        #region Query Builder
        [TestMethod]
        public void Test_Sql_QueryBuilder_All()
        {
            var builder = new SqlQueryBuilder();
            builder.SelectAll(typeof(Contact));
            builder.Table(typeof(Contact));

            var item = sqlContext.ExecuteQuery<Contact>(builder);

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_QueryBuilder_All_Join()
        {
            var builder = new SqlQueryBuilder();
            builder.SelectAll(typeof(Appointment));
            builder.Table(typeof(Contact));
            builder.AddJoin(JoinType.Inner, "Appointments", "ContactID", "Contacts", "ID");
            builder.AddWhere("Contacts", "ID", ComparisonType.Equals, 2);

            var item = sqlContext.ExecuteQuery<Appointment>(builder);

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_QueryBuilder_Update()
        {
            var builder = new SqlUpdateBuilder();
            var id = new Guid("f9c5b87a-48c4-41cd-b502-0c3c5be45b21");
            var description = "HI!";
            builder.Table(typeof(Appointment));
            builder.AddUpdate("Description", description);
            builder.AddWhere("Appointments", "ID", ComparisonType.Equals, id);

            sqlContext.ExecuteQuery<Appointment>(builder);

            var item = sqlContext.Find<Appointment>(id);

            Assert.IsTrue(item != null && item.Description == description);
        }
        #endregion

        #endregion

        #region Save And Delete
        [TestMethod]
        public void Test_Sql_Save()
        {
            var policy = new Policy
            {
                CreatedBy = "James Demeuse",
                CreatedDate = DateTime.Now,
                UpdatedBy = "James Demeuse",
                UpdatedDate = DateTime.Now,
                FeeOwnerName = "James Demeuse",
                InsuredName = "Branden Purdy",
                County = "Minnesota",
                PolicyDate = DateTime.Now,
            };

            sqlContext.SaveChanges(policy);

            Assert.IsTrue(policy.Id != 0);

            var item = sqlContext.Where<Policy>(w => w.Id == policy.Id).First<Policy>();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_Delete()
        {
            var policy = new Policy
            {
                CreatedBy = "James Demeuse",
                CreatedDate = DateTime.Now,
                UpdatedBy = "James Demeuse",
                UpdatedDate = DateTime.Now,
                FeeOwnerName = "James Demeuse",
                InsuredName = "Branden Purdy",
                County = "Minnesota",
                PolicyDate = DateTime.Now,
            };

            sqlContext.SaveChanges(policy);
            sqlContext.Delete(policy);

            var item = sqlContext.Where<Policy>(w => w.Id == policy.Id).First<Policy>();

            Assert.IsTrue(item == null);
        }

        [TestMethod]
        public void Test_Sql_InsertWithNoKey()
        {
            var link = new Linking
            {
                PolicyId = 70,
                PolicyInfoId = 70,
                Description = "Test"
            };

            sqlContext.SaveChanges(link);

            var item = sqlContext.Where<Linking>(w => w.PolicyId == link.PolicyId && w.PolicyInfoId == link.PolicyInfoId).First<Linking>();

            Assert.IsTrue(item != null);

            sqlContext.Delete(link);

            item = sqlContext.Where<Linking>(w => w.PolicyId == link.PolicyId && w.PolicyInfoId == link.PolicyInfoId).First<Linking>();

            Assert.IsTrue(item == null);
        }

        [TestMethod]
        public void Test_Sql_InsertWithGenerateInt()
        {
            var contact = new Contact
            {
                FirstName = "James",
                LastName = "Demeuse"
            };

            sqlContext.SaveChanges(contact);

            Assert.IsTrue(contact.ID != 0);

            sqlContext.Delete(contact);

            var item = sqlContext.Find<Contact>(contact.ID);

            Assert.IsTrue(item == null);
        }

        [TestMethod]
        public void Test_Sql_InsertWithGenerateGuid()
        {
            var appointment = new Appointment
            {
                Description = "Test"
            };

            sqlContext.SaveChanges(appointment);

            Assert.IsTrue(appointment.ID != Guid.Empty);

            sqlContext.Delete(appointment);

            var item = sqlContext.Find<Appointment>(appointment.ID);

            Assert.IsTrue(item == null);
        }
        #endregion

        #endregion
    }
}
