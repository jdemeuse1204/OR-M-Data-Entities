using System;
using OR_M_Data_Entities.Configuration;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface ITableFactory
    {
        ITable Find(Type type, IConfigurationOptions configuration);

        ITable Find<T>(IConfigurationOptions configuration);
    }
}
