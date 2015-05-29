using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Where;

namespace OR_M_Data_Entities.Expressions.Resolution.SubQuery
{
    public abstract class DbSubQuery<T> : DbWhereQuery<T>
    {
        protected DbSubQuery(QueryInitializerType queryInitializerType)
            : base(queryInitializerType)
        {
            
        } 
    }
}
