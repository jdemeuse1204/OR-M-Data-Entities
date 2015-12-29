using Microsoft.VisualStudio.TestTools.UnitTesting;
using OR_M_Data_Entities.Tests.Testing.Base;

namespace OR_M_Data_Entities.Tests.Testing
{
    [TestClass]
    public class SqlConcurrencyGeneralContextTests
    {
        [TestMethod]
        public void Test_Concurrency_1()
        {
            Assert.IsTrue(ConcurrencyTests.Test_1());
        }

        [TestMethod]
        public void Test_Concurrency_2()
        {
            Assert.IsTrue(ConcurrencyTests.Test_2());
        }

        [TestMethod]
        public void Test_Concurrency_3()
        {
            Assert.IsTrue(ConcurrencyTests.Test_3());
        }
    }
}
