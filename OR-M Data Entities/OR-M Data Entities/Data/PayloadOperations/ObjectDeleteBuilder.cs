using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data.PayloadOperations.LambdaResolution;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations
{
    public class ObjectDeleteBuilder : ObjectQueryBuilder
    {
        public void AddWhere<T>(Expression<Func<T, bool>> expression) where T : class
        {
            LambdaResolver.ResolveWhereExpression(expression, Map);
        }

        public void Delete<T>() where T : class
        {
            Table<T>();
        }

        public override BuildContainer Build()
        {
            throw new NotImplementedException();
        }
    }
}
