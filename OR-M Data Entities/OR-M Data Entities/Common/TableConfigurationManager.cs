using System;
using OR_M_Data_Entities.Data.Definition.Base;

namespace OR_M_Data_Entities.Common
{
    public static class TableConfigurationManager
    {
        public static TableInfo GetTableInfo(Type type)
        {
            return new TableInfo(type);
        }


        /// <summary>
        ///  Is the common method to get the table name with linked server, schema, etc
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(Type type)
        {
            var tableInfo = GetTableInfo(type);

            return tableInfo.ToString();
        }
    }
}
