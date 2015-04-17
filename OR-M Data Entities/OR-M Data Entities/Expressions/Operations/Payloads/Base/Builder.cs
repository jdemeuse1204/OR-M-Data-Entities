using OR_M_Data_Entities.Expressions.Operations.ObjectMapping.Base;

namespace OR_M_Data_Entities.Expressions.Operations.Payloads.Base
{
    public abstract class Builder
    {
        public ObjectMap Map { get; protected set; }
        protected abstract BuildContainer Build();
    }
}
