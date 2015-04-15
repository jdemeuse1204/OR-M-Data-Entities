using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.QueryResolution.Base
{
    public abstract class Resolver
    {
        protected readonly ObjectMap Map;

        protected Resolver(ObjectMap map)
        {
            Map = map;
        }

        public abstract BuildContainer Resolve();
    }
}
