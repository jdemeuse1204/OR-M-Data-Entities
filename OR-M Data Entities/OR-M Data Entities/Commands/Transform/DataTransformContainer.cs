/*
 * OR-M Data Entities v1.1.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System.Data;

namespace OR_M_Data_Entities.Commands.Transform
{
    public sealed class DataTransformContainer
    {
        public DataTransformContainer(object value, SqlDbType transformType)
        {
            Value = value;
            Transform = transformType;
        }

        public object Value { get; private set; }

        public SqlDbType Transform { get; private set; }
    }
}
