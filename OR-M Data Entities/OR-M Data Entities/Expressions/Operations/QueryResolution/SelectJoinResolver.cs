using OR_M_Data_Entities.Expressions.Operations.ObjectMapping.Base;
using OR_M_Data_Entities.Expressions.Operations.Payloads.Base;
using OR_M_Data_Entities.Expressions.Operations.QueryResolution.Base;

namespace OR_M_Data_Entities.Expressions.Operations.QueryResolution
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
