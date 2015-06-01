using OR_M_Data_Entities.Expressions.Resolution;

namespace OR_M_Data_Entities.Data.Execution
{
    public interface ISqlPayload
    {
        IExpressionQueryResolvable Query { get; }
    }
}
