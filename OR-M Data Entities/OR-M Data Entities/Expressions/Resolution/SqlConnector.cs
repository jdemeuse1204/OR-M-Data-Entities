using System;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class SqlConnector : IQueryPart
    {
        public Guid QueryId { get; set; }

        public ConnectorType Type { get; set; }
    }
}
