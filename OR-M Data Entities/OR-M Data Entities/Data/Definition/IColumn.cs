/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface IColumn
    {
        ITable Table { get; }

        Type PropertyType { get; }

        bool IsPrimaryKey { get; }

        bool IsForeignKey { get;  }

        bool IsSelectable { get; }

        bool IsPseudoKey { get; }

        bool IsList { get;  }

        bool IsNullable { get; }

        string PropertyName { get; }

        string DatabaseColumnName { get; }

        T GetCustomAttribute<T>() where T : Attribute;

        string ToString(string tableAlias, string postAppendString = "");
    }
}
