using System.Collections;
using System.Collections.Generic;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Query;

namespace OR_M_Data_Entities.Expressions
{
    public class ExpressionQuery<T> : IEnumerable<T>
    {
        public readonly DatabaseReading Context;

        private List<T> _queryResult { get; set; } 

        public DbQuery Query { get; set; }

        private bool _isLazyLoadEnabled
        {
            get { return Context != null && Context.IsLazyLoadEnabled; }
        }

        public ExpressionQuery(DatabaseReading context = null, DbQuery query = null)
        {
            Context = context;
            Query = query ?? new DbQuery();
        }

        public ExpressionQuery()
        {

        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_queryResult == null)
            {
                _queryResult = new List<T>();

                // execute query
                Query.Resolve();

                var sql = Query.Sql;
            }

            return _queryResult.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
