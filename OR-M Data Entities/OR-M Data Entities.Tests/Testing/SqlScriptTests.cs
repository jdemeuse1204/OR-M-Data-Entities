using Microsoft.VisualStudio.TestTools.UnitTesting;
using OR_M_Data_Entities.Tests.Testing.Base;
using OR_M_Data_Entities.Tests.Testing.Context;

namespace OR_M_Data_Entities.Tests.Testing
{
    [TestClass]
    public class SqlScriptTests
    {
        private readonly DefaultContext _ctx = new DefaultContext();

        [TestMethod]
        public void Test_Default_1()
        {
            Assert.IsTrue(ScriptTests.Test_1(_ctx));
        }

        [TestMethod]
        public void Test_Default_2()
        {
            Assert.IsTrue(ScriptTests.Test_2(_ctx));
        }

        [TestMethod]
        public void Test_Default_3()
        {
            Assert.IsTrue(ScriptTests.Test_3(_ctx));
        }

        [TestMethod]
        public void Test_Default_4()
        {
            Assert.IsTrue(ScriptTests.Test_4(_ctx));
        }

        [TestMethod]
        public void Test_Default_5()
        {
            Assert.IsTrue(ScriptTests.Test_1(_ctx));
        }

        [TestMethod]
        public void Test_Default_6()
        {
            Assert.IsTrue(ScriptTests.Test_6(_ctx));
        }

        [TestMethod]
        public void Test_Default_7()
        {
            Assert.IsTrue(ScriptTests.Test_7(_ctx));
        }

        [TestMethod]
        public void Test_Default_8()
        {
            Assert.IsTrue(ScriptTests.Test_8(_ctx));
        }
    }
}
