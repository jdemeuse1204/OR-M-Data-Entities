/*
 * OR-M Data Entities v1.1.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
namespace OR_M_Data_Entities.Infrastructure
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

        public string Name { get; set; }

        public object Value { get; set; }
    }
}
