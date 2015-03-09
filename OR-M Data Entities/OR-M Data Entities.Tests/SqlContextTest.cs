using Microsoft.VisualStudio.TestTools.UnitTesting;
using OR_M_Data_Entities.Tests.Context;
using OR_M_Data_Entities.Tests.Tables;

namespace OR_M_Data_Entities.Tests
{
    [TestClass]
    public class SqlContextTest
    {
        SqlContext context = new SqlContext();

        [TestMethod]
        public void TestAddPolicy()
        {
            var allPolicies = context.All<Policy>();

            Assert.IsTrue(allPolicies != null && allPolicies.Count > 0);
        }
    }
}
