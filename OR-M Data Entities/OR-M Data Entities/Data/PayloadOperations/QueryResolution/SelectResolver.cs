using System;
using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;
using OR_M_Data_Entities.Data.PayloadOperations.QueryResolution.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.QueryResolution
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
