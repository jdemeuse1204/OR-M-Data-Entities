/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

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
