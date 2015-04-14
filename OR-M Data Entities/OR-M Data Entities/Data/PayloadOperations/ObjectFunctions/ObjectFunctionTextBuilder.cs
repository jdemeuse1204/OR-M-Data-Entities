using System;
using System.Collections.Generic;
using System.Data;

namespace OR_M_Data_Entities.Data.PayloadOperations.ObjectFunctions
{
    public abstract class ObjectFunctionTextBuilder
    {
        #region Constructor
        protected ObjectFunctionTextBuilder()
        {
            _functionList = new Dictionary<int, object[]>();
        }
        #endregion

        #region Properties
        private readonly Dictionary<int, object[]> _functionList;
        public IReadOnlyDictionary<int, object[]> FunctionList
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
        #endregion
    }
}
