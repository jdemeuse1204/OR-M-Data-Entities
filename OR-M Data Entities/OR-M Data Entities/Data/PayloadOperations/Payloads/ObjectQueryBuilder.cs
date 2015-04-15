using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.Payloads
{
    public abstract class ObjectQueryBuilder : IBuilder
    {
        public ObjectMap Map { get; set; } // for selecting and renaming columns, should put together a map for easier object loading

        protected void Table<T>() where T : class
        {
            Map = new ObjectMap(typeof(T));
        }

        public abstract BuildContainer Build();
    }
}
