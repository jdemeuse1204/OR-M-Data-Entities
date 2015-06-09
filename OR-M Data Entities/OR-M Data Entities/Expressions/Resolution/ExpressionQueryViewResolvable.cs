using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class ExpressionQueryViewResolvable<T> : ExpressionQueryResolvable<T>, IExpressionQueryViewResolvable
    {
        public ExpressionQueryViewResolvable(DatabaseReading context, string viewId)
            : base(context, viewId)
        {
        }

        public ExpressionQueryViewResolvable(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
            : base(query, constructionType)
        {
        }
    }
}
