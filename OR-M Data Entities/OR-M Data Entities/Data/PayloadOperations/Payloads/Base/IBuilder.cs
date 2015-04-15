using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base
{
    public interface IBuilder
    {
        ObjectMap Map { get; set; }
        BuildContainer Build();
    }
}
