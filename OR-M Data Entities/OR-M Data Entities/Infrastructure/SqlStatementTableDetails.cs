/*
 * OR-M Data Entities v1.1.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using System;
using System.Reflection;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Infrastructure
{
    public class SqlStatementTableDetails
    {
        public SqlStatementTableDetails(Type type)
        {
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            var linkedServerAttribute = type.GetCustomAttribute<LinkedServerAttribute>();
            var tableName = tableAttribute == null ? type.Name : tableAttribute.Name;

            SelectListTableName = tableName;
            From = linkedServerAttribute != null
                ? string.Format("{0}.[{1}] AS [{1}]", linkedServerAttribute.LinkedServerText, tableName)
                : tableName;
            WhereTableName = tableName;
            GroupByTableName = tableName;
            OrderByTableName = tableName;

            LeftJoinText = string.Format("INNER JOIN {0} ON [{1}]", From, tableName);
            InnerJoinText = string.Format("LEFT JOIN {0} ON [{1}]", From, tableName);
            PlainJoinText = string.Format("JOIN {0} ON [{1}]", From, tableName);
        }

        public string SelectListTableName { get; private set; }

        public string From { get; private set; }

        public string WhereTableName { get; private set; }

        public string GroupByTableName { get; private set; }

        public string OrderByTableName { get; private set; }

        public string InnerJoinText { get; private set; }

        public string LeftJoinText { get; private set; }

        public string PlainJoinText { get; private set; }
    }
}
