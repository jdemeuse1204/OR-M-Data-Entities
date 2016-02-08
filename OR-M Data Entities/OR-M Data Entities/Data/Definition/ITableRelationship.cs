namespace OR_M_Data_Entities.Data.Definition
{
    public interface ITableRelationship
    {
        RelationshipType RelationshipType { get;  }

        string Sql { get;  }

        IMappedTable ChildTable { get; }
    }
}
