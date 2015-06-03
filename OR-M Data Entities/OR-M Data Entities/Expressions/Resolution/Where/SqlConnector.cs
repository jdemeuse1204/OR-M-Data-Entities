using System;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Where.Base;

namespace OR_M_Data_Entities.Expressions.Resolution.Where
{
    public class SqlConnector : IQueryPart
    {
        public Guid ExpressionQueryId { get; set; }

        public ConnectorType Type { get; set; }
    }
}
