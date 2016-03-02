/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using OR_M_Data_Entities.Data.Secure;

namespace OR_M_Data_Entities.Data.Definition
{
    public class SqlDbParameter
    {
        public SqlDbParameter()
        {
        }

        public SqlDbParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public SqlDbParameter(SqlSecureQueryParameter parameter)
        {
            Name = parameter.Key;
            Value = parameter.Value.Value;
        }

        public string Name { get; set; }

        public object Value { get; set; }
    }
}
