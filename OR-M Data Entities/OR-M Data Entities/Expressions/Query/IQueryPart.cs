using System;

namespace OR_M_Data_Entities.Expressions.Query
{
    public interface IQueryPart
    {
        Guid QueryId { get; set; }
    }
}
