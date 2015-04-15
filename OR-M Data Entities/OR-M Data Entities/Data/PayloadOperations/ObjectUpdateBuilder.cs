using System;
using System.Data.SqlClient;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data.PayloadOperations.LambdaResolution;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations
{
    public class ObjectUpdateBuilder : ObjectQueryBuilder
    {
        public void AddWhere<T>(Expression<Func<T, bool>> expression) where T : class
        {
            LambdaResolver.ResolveWhereExpression(expression, Map);
        }

        public void Update<T>() where T : class
        {
            Table<T>();
        }

        public void Set()
        {
            
        }

        public override BuildContainer Build()
        {
            throw new NotImplementedException();
        }
    }
}
