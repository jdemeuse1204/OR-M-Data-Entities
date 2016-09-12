/*
 * OR-M Data Entities v3.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

namespace OR_M_Data_Entities.Data.Secure
{
    public class SqlSecureQueryParameter
    {
        public string Key { get; set; }

        public string DbColumnName { get; set; }

        public string TableName { get; set; }

        public string ForeignKeyPropertyName { get; set; }

        public SqlSecureObject Value { get; set; }
    }
}
