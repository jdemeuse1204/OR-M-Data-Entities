using System.Collections.Generic;

namespace OR_M_Data_Entities.Data.Definition
{

    public interface IMappedTable
    {
        string Alias { get;  }

        string Key { get;  } // either table name or FK/PSK property name

        ITable Table { get;  }

        List<ITableRelationship> Relationships { get; }
    }
}
