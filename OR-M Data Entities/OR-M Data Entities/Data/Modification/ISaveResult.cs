/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System.Collections.Generic;
using System.Xml;

namespace OR_M_Data_Entities.Data.Modification
{
    public interface IPersistResult
    {
        XmlDocument ResultsXml { get; }

        IReadOnlyList<ITableChangeResult> Results { get; }

        bool WereChangesPersisted { get; }
    }
}
