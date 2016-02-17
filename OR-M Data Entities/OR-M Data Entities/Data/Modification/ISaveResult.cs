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
