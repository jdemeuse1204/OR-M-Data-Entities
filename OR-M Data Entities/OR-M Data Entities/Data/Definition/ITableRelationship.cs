/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

namespace OR_M_Data_Entities.Data.Definition
{
    public interface ITableRelationship
    {
        RelationshipType RelationshipType { get;  }

        string Sql { get;  }

        IMappedTable ChildTable { get; }
    }
}
