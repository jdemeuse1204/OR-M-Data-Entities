using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        #region Save
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
        #endregion

        #endregion
    }
}
