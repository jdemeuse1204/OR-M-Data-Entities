using System;
using System.Collections.Generic;
using System.Reflection;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface ITable
    {
        Type Type { get; }

        bool IsReadOnly { get; }

        bool IsUsingLinkedServer { get; }

        bool IsLookupTable { get; }

        string ClassName { get; }

        string ServerName { get; }

        string DatabaseName { get; }

        string SchemaName { get; }

        string ToString(TableNameFormat format);

        bool IsPrimaryKey(string columnName);

        bool HasForeignKeys();

        bool HasPrimaryKeysOnly();

        List<PropertyInfo> GetAllColumns();

        string GetColumnName(string propertyName);
    }
}
