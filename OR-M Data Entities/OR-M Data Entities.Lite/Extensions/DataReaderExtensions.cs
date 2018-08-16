using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Extensions
{
    internal static class DataReaderExtensions
    {
        public static dynamic Get(this IDataReader reader, string dbColumnName)
        {
            object value = reader[dbColumnName];

            if (value is DBNull) { return null; }

            return value;
        }

        public static dynamic Get(this IDataReader reader, int ordinal)
        {
            object value = reader[ordinal];

            if (value is DBNull) { return null; }

            return value;
        }
    }
}
