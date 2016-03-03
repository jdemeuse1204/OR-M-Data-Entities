using Microsoft.VisualStudio.TestTools.UnitTesting;
using OR_M_Data_Entities.Tests.Testing.Base;
using OR_M_Data_Entities.Tests.Testing.BaseESTOff;
using OR_M_Data_Entities.Tests.Testing.BaseESTOn;
using OR_M_Data_Entities.Tests.Testing.Context;

namespace OR_M_Data_Entities.Tests.Testing
{
    [TestClass]
    public class SqlConcurrencyHandleContextTests
    {
        private readonly ConcurrencyHandleContext _ctx = new ConcurrencyHandleContext();

        #region Tests
        [TestMethod]
        public void Test_ConcurrencyHandle_1()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_1(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_2()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_2(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_3()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_3(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_4()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_4(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_5()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_5(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_6()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_6(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_7()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_7(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_8()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_8(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_9()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_9(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_10()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_10(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_11()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_11(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_12()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_12(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_13()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_13(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_14()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_14(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_15()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_15(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_16()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_16(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_17()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_17(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_18()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_18(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_19()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_19(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_20()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_20(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_21()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_21(new DefaultContext()));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_22()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_22(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_23()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_23(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_24()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_24(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_25()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_25(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_26()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_26(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_27()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_27(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_28()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_28(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_29()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_29(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_30()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_30(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_31()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_31(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_32()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_32(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_33()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_33(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_34()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_34(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_35()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_35(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_36()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_36(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_37()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_37(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_38()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_38(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_39()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_39(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_40()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_40(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_41()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_41(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_42()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_42(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_43()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_43(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_44()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_44(_ctx));
        }


        [TestMethod]
        public void Test_ConcurrencyHandle_45()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_45(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_46()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_46(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_47()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_47(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_48()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_48(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_49()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_49(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_50()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_50(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_51()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_51(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_52()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_52(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_53()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_53(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_54()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_54(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_55()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_55(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_56()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_56(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_57()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_57(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_58()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_58(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_59()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_59(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_60()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_60(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_61()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_61(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_62()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_62(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_63()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_63(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_64()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_64(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_65()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_65(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_66()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_66(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_67()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_67(_ctx));
        }


        [TestMethod]
        public void Test_ConcurrencyHandle_68()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_68(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_69()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_Extra_1(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_70()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_Extra_2(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_71()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_Extra_3(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_72()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_1(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_73()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_2(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_74()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_3(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_75()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_4(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_76()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_5(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_77()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_6(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_78()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_7(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_79()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_8(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_80()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_9(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_81()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_10(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_82()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_11(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_83()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_12(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_84()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_13(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_85()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_14(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_86()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_15(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_87()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_16(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_88()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_17(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_89()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_18(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_90()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_19(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_91()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_20(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_92()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_21(new DefaultContext()));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_93()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_22(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_94()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_23(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_95()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_24(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_96()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_25(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_97()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_26(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_98()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_27(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_99()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_28(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_100()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_29(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_101()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_30(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_102()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_31(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_103()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_32(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_104()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_33(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_105()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_34(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_106()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_35(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_107()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_36(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_108()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_37(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_109()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_38(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_110()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_39(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_111()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_40(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_112()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_41(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_113()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_42(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_114()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_43(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_115()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_44(_ctx));
        }


        [TestMethod]
        public void Test_ConcurrencyHandle_116()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_45(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_117()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_46(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_118()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_47(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_119()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_48(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_120()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_49(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_121()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_50(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_122()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_51(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_123()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_52(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_124()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_53(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_125()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_54(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_126()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_55(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_127()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_56(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_128()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_57(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_129()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_58(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_130()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_59(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_131()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_60(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_132()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_61(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_133()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_62(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_134()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_63(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_135()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_64(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_136()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_65(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_137()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_66(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_138()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_67(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_139()
        {
            Assert.IsTrue(DefaultTestsESTOn.Test_68(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_140()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_69(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_141()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_70(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_142()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_71(_ctx));
        }

        [TestMethod]
        public void Test_ConcurrencyHandle_143()
        {
            Assert.IsTrue(DefaultTestsESTOff.Test_72(_ctx));
        }
        #endregion
    }
}
