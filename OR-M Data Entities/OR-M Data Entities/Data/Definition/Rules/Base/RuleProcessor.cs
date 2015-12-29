using System;

namespace OR_M_Data_Entities.Data.Definition.Rules.Base
{
    public static class RuleProcessor
    {
        public static void ProcessRule<T>(params object[] constructorArgs) 
            where T : IRule
        {
            var instance = (T)Activator.CreateInstance(typeof (T), constructorArgs);

            instance.Process();
        } 
    }
}
