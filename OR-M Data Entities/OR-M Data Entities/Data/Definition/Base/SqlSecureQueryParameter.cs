﻿/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using OR_M_Data_Entities.Data.Secure;

namespace OR_M_Data_Entities.Data.Definition.Base
{
    public class SqlSecureQueryParameter
    {
        public string Key { get; set; }

        public string DbColumnName { get; set; }

        public SqlSecureObject Value { get; set; }
    }
}
