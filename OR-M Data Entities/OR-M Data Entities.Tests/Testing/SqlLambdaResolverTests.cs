using Microsoft.VisualStudio.TestTools.UnitTesting;
using OR_M_Data_Entities.Tests.Testing.Base;
using OR_M_Data_Entities.Tests.Testing.Context;

namespace OR_M_Data_Entities.Tests.Testing
{
 [TestClass]
    public class SqlLambdaResolverTests
    {
        private readonly DefaultContext _ctx = new DefaultContext();

        [TestMethod]
        public void Test_LambdaResolver_1()
        {
            Assert.IsTrue(LambdaResolverTests.Test_1(_ctx));
        }

        [TestMethod]
        public void Test_LambdaResolver_2()
        {
            Assert.IsTrue(LambdaResolverTests.Test_2(_ctx));
        }

        [TestMethod]
        public void Test_LambdaResolver_3()
        {
            Assert.IsTrue(LambdaResolverTests.Test_3(_ctx));
        }

        [TestMethod]
        public void Test_LambdaResolver_4()
        {
            Assert.IsTrue(LambdaResolverTests.Test_4(_ctx));
        }

        [TestMethod]
        public void Test_LambdaResolver_5()
        {
            Assert.IsTrue(LambdaResolverTests.Test_5(_ctx));
        }

        [TestMethod]
        public void Test_LambdaResolver_6()
        {
            Assert.IsTrue(LambdaResolverTests.Test_6(_ctx));
        }

        [TestMethod]
        public void Test_LambdaResolver_7()
        {
            Assert.IsTrue(LambdaResolverTests.Test_7(_ctx));
        }

        [TestMethod]
        public void Test_LambdaResolver_8()
        {
            Assert.IsTrue(LambdaResolverTests.Test_8(_ctx));
        }

        [TestMethod]
        public void Test_LambdaResolver_9()
        {
            Assert.IsTrue(LambdaResolverTests.Test_9(_ctx));
        }

        [TestMethod]
        public void Test_LambdaResolver_10()
        {
            Assert.IsTrue(LambdaResolverTests.Test_10(_ctx));
        }
    }
}
