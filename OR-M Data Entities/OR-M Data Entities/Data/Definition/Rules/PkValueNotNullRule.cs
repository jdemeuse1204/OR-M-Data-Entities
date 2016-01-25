/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Reflection;
using OR_M_Data_Entities.Data.Definition.Rules.Base;
using OR_M_Data_Entities.Exceptions;

namespace OR_M_Data_Entities.Data.Definition.Rules
{
    public sealed class PkValueNotNullRule : IRule
    {
        private readonly object _pkValue;

        private readonly MemberInfo _key;

        public PkValueNotNullRule(object pkValue, MemberInfo key)
        {
            _pkValue = pkValue;
            _key = key;
        }

        public void Process()
        {
            if (_pkValue == null) throw new SqlSaveException(string.Format("Primary Key cannot be null: {0}", _key.GetColumnName()));
        }
    }
}
