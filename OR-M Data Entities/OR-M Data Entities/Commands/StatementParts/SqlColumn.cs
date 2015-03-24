using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.StatementParts
{
    public class SqlColumn
    {
        #region Constructor
        public SqlColumn(MemberInfo columnInfo)
            : this()
        {
            Column = columnInfo;
        }

        public SqlColumn()
        {
            _functionList = new Dictionary<int, object[]>();
        }
        #endregion

        #region Properties
        public MemberInfo Column { get; set; }

        public SqlDbType DataType { get; set; }

        private readonly Dictionary<int, object[]> _functionList;
        public IEnumerable<KeyValuePair<int, object[]>> FunctionList
        {
            get { return _functionList; }
        }
        #endregion

        #region Methods
        public void AddFunction(Func<object, SqlDbType, object> function, SqlDbType dataType)
        {
            _functionList.Add(_functionList.Count + 1, new object[] { function, dataType });
        }

        public void AddFunction(Func<SqlDbType, object, int?, object> function, SqlDbType dataType, int? style)
        {
            _functionList.Add(_functionList.Count + 1, new object[] { function, dataType, style });
        }

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
            var result = string.Format("[{0}].[{1}]", tableName, columnName);

            foreach (var function in _functionList)
            {
                var functionName = ((dynamic)function.Value[0]).Method.Name.ToUpper();
                var alias = (includeAlias ? string.Format(" as '{0}'", columnName) : string.Empty);

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
