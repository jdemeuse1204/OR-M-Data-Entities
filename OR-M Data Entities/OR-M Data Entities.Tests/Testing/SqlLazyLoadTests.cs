using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OR_M_Data_Entities.Tests.Testing.Base;
using OR_M_Data_Entities.Tests.Testing.Context;

namespace OR_M_Data_Entities.Tests.Testing
{
    [TestClass]
    public class SqlLazyLoadTests
    {
        private readonly LazyLoadContext _ctx = new LazyLoadContext();

        [TestMethod]
        public void Test_LazyLoading_1()
        {
            Assert.IsTrue(LazyLoadTests.Test_1(_ctx));
        }

        [TestMethod]
        public void Test_LazyLoading_2()
        {
            Assert.IsTrue(LazyLoadTests.Test_2(_ctx));
        }

        [TestMethod]
        public void Test_LazyLoading_3()
        {
            Assert.IsTrue(LazyLoadTests.Test_3(_ctx));
        }

        [TestMethod]
        public void Test_LazyLoading_4()
        {
            Assert.IsTrue(LazyLoadTests.Test_4(_ctx));
        }

        [TestMethod]
        public void Test_LazyLoading_5()
        {
            Assert.IsTrue(LazyLoadTests.Test_5(_ctx));
        }

        [TestMethod]
        public void Test_LazyLoading_6()
        {
            Assert.IsTrue(LazyLoadTests.Test_6(_ctx));
        }

        [TestMethod]
        public void Test_LazyLoading_7()
        {
            Assert.IsTrue(LazyLoadTests.Test_7(_ctx));
        }
    }
}
