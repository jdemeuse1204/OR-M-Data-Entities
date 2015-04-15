using System.Linq;
using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;
using OR_M_Data_Entities.Data.PayloadOperations.QueryResolution.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.QueryResolution
{
    public class ForeignKeySelectWhereJoinResolver : Resolver
    {
        public ForeignKeySelectWhereJoinResolver(ObjectMap map)
            : base(map)
        {
        }

        public override BuildContainer Resolve()
        {
            var select = Map.Rows.HasValue ? string.Format("SELECT TOP {0}", Map.Rows.Value) : "SELECT ";

            var columns = Map.Tables.Aggregate(string.Empty, (current, table) => current + table.GetSelectColumns());

            var joins = Map.Tables.Aggregate(string.Empty, (current, table) => current + table.GetJoins());

            select += columns;
            select += joins;

            return null;
        }
    }
}
