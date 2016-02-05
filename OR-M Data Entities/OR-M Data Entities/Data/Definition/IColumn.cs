using System;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface IColumn
    {
        ITable Table { get; }

        Type PropertyType { get; }

        bool IsPrimaryKey { get; }

        bool IsForeignKey { get;  }

        bool IsPseudoKey { get; }

        bool IsList { get;  }

        bool IsNullable { get; }

        string PropertyName { get; }

        string DatabaseColumnName { get; }

        T GetCustomAttribute<T>() where T : Attribute;
    }
}
