using System;
using System.Collections.Generic;
using System.Data;

namespace OR_M_Data_Entities.Commands.StatementParts
{
    public class SqlValue
    {
        #region Constructor
        public SqlValue(object value, SqlDbType dataType)
        {
            Value = value;
            DataType = dataType;
            _functionList = new Dictionary<int, object>();
        }
        #endregion

        #region Properties
        public object Value { get; set; }

        public SqlDbType DataType { get; set; }

        private readonly Dictionary<int, object> _functionList;
        public IEnumerable<KeyValuePair<int, object>> FunctionList
        {
            get { return _functionList; }
        }
        #endregion

        #region Methods
        public void AddFunction(Func<object, SqlDbType, object> function)
        {
            _functionList.Add(_functionList.Count + 1, function);
        }

        public void AddFunction(Func<SqlDbType, object, int?, object> function)
        {
            _functionList.Add(_functionList.Count + 1, function);
        }

        public string GetValueText(string parameterName)
        {
            var result = string.Format("{0}", parameterName);

            foreach (var function in _functionList)
            {
                var name = function.Value;
                result = string.Format("{0}{1}{2}", "", result, "");
            }

            return result;
        }
        #endregion
    }
}
