using System.Collections.Generic;
using OR_M_Data_Entities.Data.Definition;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public interface IExpressionQueryResolvable
    {
        void ResolveExpression();

        IReadOnlyCollection<SqlDbParameter> Parameters { get; }

        string Sql { get; }
    }
}
