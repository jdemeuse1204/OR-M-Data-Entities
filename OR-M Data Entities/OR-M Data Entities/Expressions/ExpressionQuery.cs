using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution;

namespace OR_M_Data_Entities.Expressions
{
    public abstract class ExpressionQuery<T> : DbQuery<T>, IEnumerable<T>, IExpressionQuery
    {
        #region Properties
        private List<T> _queryResult { get; set; }
        #endregion

        #region Constructor
        protected ExpressionQuery(DatabaseReading context)
            : base(context)
        {
            
        }

        protected ExpressionQuery(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
            : base(query, constructionType)
        {
            
        }
        #endregion

        #region Enumeration Methods
        public IEnumerator<T> GetEnumerator()
        {
            if (IsSubQuery)
            {
                this.GetType().GetMethod("ResolveExpression", BindingFlags.Public | BindingFlags.Instance).Invoke(this, null); 
                return null;
            }

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
