using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base
{
    public abstract class Builder
    {
        public ObjectMap Map { get; protected set; }
        protected abstract BuildContainer Build();
    }
}
