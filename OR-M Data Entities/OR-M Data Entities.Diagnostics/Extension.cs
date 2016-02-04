using System;
using System.Reflection;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Diagnostics
{
    public static class Extension
    {
        public static string GetSystemTableSqlString(this Type type)
        {
            return string.Format("{0}Tables", _getSystemSqlString(type));
        }

        public static string GetSystemColumnsSqlString(this Type type)
        {
            return string.Format("{0}Columns", _getSystemSqlString(type));
        }

        private static string _getSystemSqlString(this Type type)
        {
            var linkedServerAttribute = type.GetCustomAttribute<LinkedServerAttribute>();

            return linkedServerAttribute == null
                ? "Sys."
                : string.Format("[{0}].[{1}].Sys.", linkedServerAttribute.ServerName,
                    linkedServerAttribute.DatabaseName);
        }

        public static string GetSql<T>(this ExpressionQuery<T> query)
        {
            var resolvableQuery = (IExpressionQueryResolvable)query;

            // execute query
            resolvableQuery.ResolveExpression();

            return resolvableQuery.Sql;
        }
    }
}
