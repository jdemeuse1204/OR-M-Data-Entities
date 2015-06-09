using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class OrderedExpressionQueryResolvable<T> : ExpressionQueryResolvable<T>, IOrderedExpressionQueryResolvable
    {
        public OrderedExpressionQueryResolvable(DatabaseReading context)
            : base(context)
        {
        }

        public OrderedExpressionQueryResolvable(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
            : base(query, constructionType)
        {
        }
    }
}
