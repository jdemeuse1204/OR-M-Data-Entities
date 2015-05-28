using System.Collections;
using System.Collections.Generic;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Query;

namespace OR_M_Data_Entities.Expressions
{
    public abstract class ExpressionQuery<T> : DbQuery<T>, IEnumerable<T>
    {
        #region Fields
        public readonly DatabaseReading Context;
        #endregion

        #region Properties
        private List<T> _queryResult { get; set; } 

        private bool _isLazyLoadEnabled
        {
            get { return Context != null && Context.IsLazyLoadEnabled; }
        }
        #endregion

        #region Constructor
        protected ExpressionQuery(DatabaseReading context = null)
        {
            Context = context;
        }
        #endregion

        #region Enumeration Methods
        public IEnumerator<T> GetEnumerator()
        {
            if (_queryResult != null) return _queryResult.GetEnumerator();

            _queryResult = new List<T>();

            _queryResult = Context.ExecuteQuery(this).ToList();

            return _queryResult.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
