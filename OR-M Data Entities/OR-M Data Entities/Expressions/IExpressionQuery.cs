using System;

namespace OR_M_Data_Entities.Expressions
{
    public interface IExpressionQuery
    {
        Type Type { get; }

        string Sql { get; }
    }
}
