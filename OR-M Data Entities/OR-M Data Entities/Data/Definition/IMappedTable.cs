using System.Collections.Generic;

namespace OR_M_Data_Entities.Data.Definition
{

    public interface IMappedTable
    {
        string Alias { get; }

        string Key { get; } // either table name or FK/PSK property name

        ITable Table { get; }

        // all selected columns
        HashSet<IMappedColumn> MappedColumns { get; }

        HashSet<ITableRelationship> Relationships { get; }

        void Clear();

        void Select(string propertyName, int ordinal);

        string Sql { get; }

        int SelectAll(int startingOrdinal);
    }
}
