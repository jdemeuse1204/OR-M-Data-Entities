using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Collections;
using OR_M_Data_Entities.Expressions.Query.Columns;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public interface IExpressionQueryResolvable
    {
        void ResolveExpression();

        IReadOnlyList<SqlDbParameter> Parameters { get; }

        ReadOnlyTableCollection Tables { get; }

        // for use with payload
        IEnumerable<DbColumn> SelectInfos { get; }

        void Clear();

        bool IsSubQuery { get; }

        bool IsLazyLoading { get; }

        string Sql { get; }

        void Initialize();

        Guid Id { get; }

        ExpressionQueryConstructionType ConstructionType { get; }
    }
}
