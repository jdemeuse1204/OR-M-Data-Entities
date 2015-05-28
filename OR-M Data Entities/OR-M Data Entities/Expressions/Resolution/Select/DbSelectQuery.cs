using OR_M_Data_Entities.Expressions.Resolution.Containers;

namespace OR_M_Data_Entities.Expressions.Resolution.Select
{
    public abstract class DbSelectQuery<T> 
    {
        protected DbSelectQuery()
            : base()
        {
            
        }

        public readonly SelectInfoResolutionContainer SelectList;
    }
}
