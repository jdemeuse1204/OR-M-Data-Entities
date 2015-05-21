using System.Collections;
using System.Collections.Generic;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Query;

namespace OR_M_Data_Entities.Expressions
{
    public class ExpressionQuery<T> : IEnumerable<T>, IExpressionQueryable
    {
        #region Fields
        public readonly DatabaseReading Context;
        #endregion

        #region Properties
        private List<T> _queryResult { get; set; } 

        public DbQuery Query { get; set; }

        private bool _isLazyLoadEnabled
        {
            get { return Context != null && Context.IsLazyLoadEnabled; }
        }
        #endregion

        #region Constructor
        public ExpressionQuery(DatabaseReading context = null, DbQuery query = null)
        {
            Context = context;
            Query = query ?? new DbQuery();
        }

        public ExpressionQuery()
        {

        }
        #endregion

        #region Methods
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
