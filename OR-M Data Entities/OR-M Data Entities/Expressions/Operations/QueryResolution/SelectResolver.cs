using OR_M_Data_Entities.Expressions.Operations.ObjectMapping.Base;
using OR_M_Data_Entities.Expressions.Operations.Payloads.Base;
using OR_M_Data_Entities.Expressions.Operations.QueryResolution.Base;

namespace OR_M_Data_Entities.Expressions.Operations.QueryResolution
{
    public class SelectResolver : Resolver
    {
        public SelectResolver(ObjectMap map)
            : base(map)
        {
        }

        public override BuildContainer Resolve()
        {
            var result = new BuildContainer();
            var select = ResolveSelect();
            var from = ResolveFrom();
            var columns = ResolveColumns();

            result.Sql = string.Format("{0}{1} FROM {2}",
                select,
                columns.TrimEnd(','),
                string.Format("[{0}] ", from));

            return result;
        }
    }
}
