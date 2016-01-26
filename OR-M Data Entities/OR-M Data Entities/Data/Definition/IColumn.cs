namespace OR_M_Data_Entities.Data.Definition
{
    public interface IColumn
    {
        ITable Table { get; }

        bool IsPrimaryKey { get; }

        bool IsForeignKey { get;  }

        bool IsPseudoKey { get; }

        bool IsList { get;  }

        string PropertyName { get; }

        string DatabaseColumnName { get; }
    }
}
