using FastMember;
using OR_M_Data_Entities.Lite.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace OR_M_Data_Entities.Lite.Extensions
{
    public static class AttributeExtensions
    {
        internal static IEnumerable<T> GetAttributes<T>(this Type type)
        {
            return type.GetCustomAttributes(typeof(T), false).Cast<T>();
        }

        internal static T GetAttribute<T>(this Type type)
        {
            return type.GetAttributes<T>().FirstOrDefault();
        }

        internal static T GetAttribute<T>(this Member member)
        {
            return member.GetAttribute(typeof(T), false) as dynamic;
        }

        internal static bool IsList(this Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        internal static Type Resolve(this Type type)
        {
            return type.IsList() || type.IsNullableType()
                ? type.GetGenericArguments()[0]
                : type;
        }

        internal static bool IsNullableType(this Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        internal static IPeekDataReader ExecutePeekReader(this SqlCommand command)
        {
            return new PeekDataReader(command.ExecuteReader(System.Data.CommandBehavior.CloseConnection));
        }
    }
}
