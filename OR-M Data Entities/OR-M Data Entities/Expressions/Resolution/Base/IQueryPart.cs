using System;

namespace OR_M_Data_Entities.Expressions.Resolution.Base
{
    public interface IQueryPart
    {
        Guid ExpressionQueryId { get; set; }
    }
}
