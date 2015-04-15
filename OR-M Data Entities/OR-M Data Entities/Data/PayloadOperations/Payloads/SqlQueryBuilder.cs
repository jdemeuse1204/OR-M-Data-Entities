using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.Payloads
{
    public abstract class SqlQueryBuilder : IBuilder
    {
        public abstract BuildContainer Build();
    }
}
