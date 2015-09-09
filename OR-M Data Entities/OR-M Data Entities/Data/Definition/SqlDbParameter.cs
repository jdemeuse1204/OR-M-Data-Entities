/*
 * OR-M Data Entities v2.2
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using OR_M_Data_Entities.Commands.Transform;

namespace OR_M_Data_Entities.Data.Definition
{
    public class SqlDbParameter
    {
        public SqlDbParameter()
        {
            Transform = new TransformContainer();
        }

        public SqlDbParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public object Value { get; set; }

        public TransformContainer Transform { get; set; }
    }
}
