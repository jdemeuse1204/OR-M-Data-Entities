using Microsoft.VisualStudio.TestTools.UnitTesting;
using OR_M_Data_Entities.Tests.Testing.Base;
using OR_M_Data_Entities.Tests.Testing.Context;

namespace OR_M_Data_Entities.Tests.Testing
{
    [TestClass]
    public class KeyChangeContextTests
    {
        private readonly InsertKeyChangeContext _ctx = new InsertKeyChangeContext();

        [TestMethod]
        public void Test_1()
        {
            Assert.IsTrue(OtherTests.Test_1(_ctx));
        }

        [TestMethod]
        public void Test_2()
        {
            Assert.IsTrue(OtherTests.Test_2(_ctx));
        }
    }
}
