using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            // make sure it does not exist after
            var policy = _addPolicy();

            var foundEntity = ctx.Find<Policy>(policy.Id);

            Assert.IsTrue(foundEntity != null);
        }

        [TestMethod]
        public void Test_5()
        {
            // Test first or default method
            // make sure it does not exist after
            var policy = _addPolicy();

            var foundEntity = ctx.From<Policy>().FirstOrDefault(w => w.Id == policy.Id);

            Assert.IsTrue(foundEntity != null);
        }

        [TestMethod]
        public void Test_6()
        {
            // Test where with first or default method
            // make sure it does not exist after
            var policy = _addPolicy();

            var foundEntity = ctx.From<Policy>().Where(w => w.Id == policy.Id).FirstOrDefault();

            Assert.IsTrue(foundEntity != null);
        }

        [TestMethod]
        public void Test_7()
        {
            // Delete one record, no foreign keys
            // make sure it does not exist after
            var policy = _addPolicy();

            ctx.Delete(policy);

            var foundEntity = ctx.Find<Policy>(policy.Id);

            Assert.IsTrue(foundEntity == null);
        }

        [TestMethod]
        public void Test_8()
        {
            // Test Disconnect with expression query
            // make sure it does not exist after
            var policy = _addPolicy();

            var foundEntity = ctx.From<Policy>().Where(w => w.Id == policy.Id).FirstOrDefault();

            var state = ctx.GetConnectionState();

            Assert.AreEqual(state, ConnectionState.Closed);
        }

        [TestMethod]
        public void Test_9()
        {
            // Test contains in expression query
            // make sure it does not exist after
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
}
