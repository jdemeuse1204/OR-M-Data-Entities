using System.Collections;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Query;

namespace OR_M_Data_Entities.Expressions
{
    public class ExpressionQuery<T> : IEnumerable
    {
        public readonly DatabaseReading Context;

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

        public IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}
