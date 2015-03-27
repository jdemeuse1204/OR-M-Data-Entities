/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using System.Data;
using System.Reflection;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.StatementParts
{
    public class SqlColumn : SqlFunctionTextBuilder
    {
        #region Constructor
        public SqlColumn(MemberInfo columnInfo)
            : this()
        {
            Column = columnInfo;
        }

        public SqlColumn()
        {
        }
        #endregion

        #region Properties
        public MemberInfo Column { get; set; }

        public SqlDbType DataType { get; set; }
        #endregion

        #region Methods
        public string GetColumnName()
        {
            return DatabaseSchemata.GetColumnName(Column);
        }

        public string GetColumnText(string tableName)
        {
            return _getColumnText(tableName, false);
        }

        public string GetColumnTextWithAlias(string tableName)
        {
            return _getColumnText(tableName, true);
        }

        public string _getColumnText(string tableName, bool includeAlias)
        {
            var columnName = GetColumnName();
            var aliasText = string.Format("{0}{1}", tableName, columnName);
            var alias = (includeAlias ? string.Format(" as [{0}]", aliasText) : string.Empty);
            var result = string.Format("[{0}].[{1}]{2}", tableName, columnName, alias);

            foreach (var function in FunctionList)
            {
                var functionName = ((dynamic)function.Value[0]).Method.Name.ToUpper();
                

                switch ((string)functionName)
                {
                    case "CAST":
                        result = string.Format(" CAST({0} as {1}) {2}",
                            result,
                            function.Value[1],
                            alias);
                        break;
                    case "CONVERT":
                        result = string.Format("CONVERT({0},{1},{2}) {3}",
                            function.Value[1],
                            result,
                            (function.Value[2] == null ? string.Empty : function.Value[2].ToString()),
                            alias);
                        break;
                }
            }

            return result;
        }

        #endregion
    }
}
