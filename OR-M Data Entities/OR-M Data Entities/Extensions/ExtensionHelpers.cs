/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OR_M_Data_Entities.Extensions
{
    public class ForeignKeyGetResult
    {
        public ForeignKeyGetResult(List<PropertyInfo> foreignKeys, List<Type> typesToSkip)
        {
            ForeignKeys = foreignKeys;
            TypesToSkip = typesToSkip;
        }

        public readonly IEnumerable<PropertyInfo> ForeignKeys;

        public readonly IEnumerable<Type> TypesToSkip;
    }
}
