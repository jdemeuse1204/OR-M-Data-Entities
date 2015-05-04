/*
 * OR-M Data Entities v1.1.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace OR_M_Data_Entities.Expressions.Resolver
{
    public sealed class ExpressionTypeTransform
    {
        public static bool IsList(object o)
        {
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static SqlDbType GetSqlDbType(Type type)
        {
            var name = type.Name;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                name = type.GetGenericArguments()[0].Name;
            }

            switch (name.ToUpper())
            {
                case "INT64":
                    return SqlDbType.BigInt;
                case "INT32":
                    return SqlDbType.Int;
                case "BYTE":
                    return SqlDbType.TinyInt;
                case "BOOLEAN":
                    return SqlDbType.Bit;
                case "STRING":
                    return SqlDbType.VarChar;
                case "DATETIME":
                    return SqlDbType.DateTime;
                case "DATETIMEOFFSET":
                    return SqlDbType.DateTimeOffset;
                case "DECIMAL":
                    return SqlDbType.Decimal;
                case "SINGLE":
                    return SqlDbType.Real;
                case "INT16":
                    return SqlDbType.SmallInt;
                case "TIMESPAN":
                    return SqlDbType.Time;
                case "GUID":
                    return SqlDbType.UniqueIdentifier;
                case "XML":
                    return SqlDbType.Xml;
                case "DOUBLE":
                    return SqlDbType.Float;
                case "BYTE[]":
                    return SqlDbType.Binary;
                case "CHAR":
                    return SqlDbType.Char;
                case "OBJECT":
                    return SqlDbType.Variant;
                default:
                    throw new Exception(string.Format("Type of {0} not recognized!", name));
            }
        }
    }
}
