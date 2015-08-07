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
