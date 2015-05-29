using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Expressions.Resolution.Select;

namespace OR_M_Data_Entities.Expressions.Resolution.Join
{
    public abstract class DbJoinQuery<T> : DbSelectQuery<T>
    {
        public readonly JoinResolutionContainer JoinResolution;

        protected DbJoinQuery(QueryInitializerType queryInitializerType)
            : base(queryInitializerType)
        {
            JoinResolution = new JoinResolutionContainer(ForeignKeyJoinPairs);
        }        
    }
}
