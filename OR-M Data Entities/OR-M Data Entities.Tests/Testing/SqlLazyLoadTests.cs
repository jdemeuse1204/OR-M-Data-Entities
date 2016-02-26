using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OR_M_Data_Entities.Tests.Testing.Base;

namespace OR_M_Data_Entities.Tests.Testing
{
    [TestClass]
    public class SqlLazyLoadTests
    {
        [TestMethod]
        public void Test_LazyLoading_1()
        {
            Assert.IsTrue(ConcurrencyTests.Test_1());
        }
    }
}
