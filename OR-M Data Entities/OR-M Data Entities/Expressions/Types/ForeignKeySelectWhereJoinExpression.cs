using OR_M_Data_Entities.Expressions.Types.Base;

namespace OR_M_Data_Entities.Expressions.Types
{
    public class ForeignKeySelectWhereJoinExpression : SqlExpressionResolvableBase
    {
        public ForeignKeySelectWhereJoinExpression(ExpressionQuery query)
            : base(query)
        {
            
        }

        public override void Resolve()
        {
            throw new System.NotImplementedException();
        }
    }
}
