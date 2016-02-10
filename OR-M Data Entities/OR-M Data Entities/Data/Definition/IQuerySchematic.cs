﻿using System;
using System.Collections.Generic;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface IQuerySchematic
    {
        Type Key { get; }

        List<IMappedTable> Map { get; }

        IMappedTable FindTable(Type type);

        IMappedTable FindTable(string tableKey);
    }
}