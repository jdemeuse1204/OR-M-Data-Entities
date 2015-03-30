using OR_M_Data_Entities.Expressions.Types.Base;

namespace OR_M_Data_Entities.Expressions.Types
{
    public class ForeignKeySelectWhereExpression : SqlExpressionResolvableBase
    {
        public ForeignKeySelectWhereExpression(ExpressionQuery query)
            : base(query)
        {
            
        }

        public override void Resolve()
        {
            throw new System.NotImplementedException();
        }
    }
}
