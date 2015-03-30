using OR_M_Data_Entities.Expressions.Types.Base;

namespace OR_M_Data_Entities.Expressions.Types
{
    public class SelectWhereExpression : SqlExpressionResolvableBase
    {
        public SelectWhereExpression(ExpressionQuery query)
            : base(query)
        {
            
        }

        public override void Resolve()
        {
            throw new System.NotImplementedException();
        }
    }
}
