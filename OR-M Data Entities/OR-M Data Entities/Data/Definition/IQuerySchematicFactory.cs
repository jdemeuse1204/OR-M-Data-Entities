using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Configuration;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface IQuerySchematicFactory
    {
        IQuerySchematic FindAndReset(ITable table, IConfigurationOptions configuration, ITableFactory tableFactory);

        IQuerySchematic CreateTemporarySchematic(List<Type> types, IConfigurationOptions configuration,
            ITableFactory tableFactory, Type selectedType);
    }
}
