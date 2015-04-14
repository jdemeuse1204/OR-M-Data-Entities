using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;

namespace OR_M_Data_Entities.Data.PayloadOperations
{
    public class ObjectSelectBuilder : ObjectQueryBuilder
    {
        private List<object> _whereExpressions { get; set; } 
        private List<object> _innerJoinExpressions { get; set; }
        private List<object> _leftJoinExpressions { get; set; } 

        public ObjectSelectBuilder(SqlConnection connection)
            : base(connection)
        {
            _whereExpressions = new List<object>();
            _innerJoinExpressions = new List<object>();
            _leftJoinExpressions = new List<object>();
        }

        public void Select<T>() where T : class
        {
            // rename for asethetics
            Table<T>();
        }

        public void Where<T>(Expression<Func<T, bool>> expression) where T : class
        {
            _whereExpressions.Add(expression);
        }

        public void InnerJoin<TParent, TChild>(Expression<Func<TParent, TChild, bool>> expression)
            where TParent : class
            where TChild : class
        {
            _innerJoinExpressions.Add(expression);
        }

        public void LeftJoin<TParent, TChild>(Expression<Func<TParent, TChild, bool>> expression)
            where TParent : class
            where TChild : class
        {
            _leftJoinExpressions.Add(expression);
        }

        public override string Resolve()
        {
            throw new System.NotImplementedException();
        }
    }
}
