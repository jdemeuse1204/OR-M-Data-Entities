using System;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        private readonly int TestID = 63;
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
        public void Test_Sql_DataReader_Select()
        {
            var sql = "Select * From Policy";

            var item = sqlContext.ExecuteQuery<Policy>(sql).Select();

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
        public void Test_Sql_Where_ExpressionQuery_Cast()
        {
            var currentDateTime = DateTime.Now;
            var item = sqlContext.From<Policy>()
                .Where<Policy>(w => Cast.As(w.CreatedDate, SqlDbType.Date) == Cast.As(currentDateTime, SqlDbType.Date))
                .Select<Policy>()
                .First<Policy>();

            Assert.IsTrue(item != null);
        }

        [TestMethod]
        public void Test_Sql_Where_ExpressionQuery_Convert()
        {
            var currentDateTime = DateTime.Now;
            var item = sqlContext.From<Policy>()
                .Where<Policy>(w => Cast.As(w.CreatedDate, SqlDbType.Date) == Cast.As(currentDateTime, SqlDbType.Date))
                .Select<Policy>(w => Conversion.To(SqlDbType.VarChar, w.CreatedDate, 101))
                .First<string>();

            Assert.IsTrue(item != null);
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
