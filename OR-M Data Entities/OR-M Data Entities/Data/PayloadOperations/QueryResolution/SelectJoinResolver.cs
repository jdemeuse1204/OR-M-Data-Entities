using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;
using OR_M_Data_Entities.Data.PayloadOperations.QueryResolution.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.QueryResolution
{
    public class SelectJoinResolver : Resolver
    {
        public SelectJoinResolver(ObjectMap map)
            : base(map)
        {
        }

        public override BuildContainer Resolve()
        {
            var result = new BuildContainer();
            var select = ResolveSelect();
            var from = ResolveFrom();
            var columns = ResolveColumns();
            var joins = ResolveJoins();

            result.Sql = string.Format("{0}{1} FROM {2}{3}",
                select,
                columns.TrimEnd(','),
                string.Format("[{0}] ", from),
                joins);

            return result;
        }
    }
}
