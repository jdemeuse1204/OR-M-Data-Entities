using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Expressions.Resolution.Join;

namespace OR_M_Data_Entities.Expressions.Resolution.Where
{
    public abstract class DbWhereQuery<T> : DbJoinQuery<T>
    {
        public readonly WhereResolutionContainer WhereResolution;

        protected DbWhereQuery()
            : base()
        {
            
        }  
    }
}
