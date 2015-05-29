using OR_M_Data_Entities.Expressions.Resolution;

namespace OR_M_Data_Entities.Data.Execution
{
    public class SqlPayload : ISqlPayload
    {
        public SqlPayload(IExpressionQueryResolvable query)
        {
            Query = query;
        }

        public IExpressionQueryResolvable Query { get; private set; }

        public bool IsLazyLoading { get; private set; }
    }
}
